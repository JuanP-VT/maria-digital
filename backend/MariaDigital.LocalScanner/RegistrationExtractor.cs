using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MariaDigital.Domain.Students;

namespace MariaDigital.LocalScanner;

/// <summary>
/// Sends a sheet photo to Ollama's native chat API (not the OpenAI-compatible one, which cannot
/// turn thinking off) and gets back a <see cref="RegistrationForm"/> constrained by a JSON schema.
/// </summary>
public class RegistrationExtractor
{
	public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		Converters = { new JsonStringEnumConverter() },
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};

	private readonly HttpClient _http;

	public RegistrationExtractor(HttpClient http)
	{
		_http = http;
	}

	public static JsonNode BuildSchema() =>
		JsonSchemaExporter.GetJsonSchemaAsNode(JsonOptions, typeof(RegistrationForm));

	public static JsonObject BuildRequest(
		string model,
		byte[] image,
		string instructions,
		string userMessage,
		JsonNode schema
	) =>
		new()
		{
			["model"] = model,
			["stream"] = false,
			["think"] = false,
			["format"] = schema,
			["options"] = new JsonObject { ["temperature"] = 0, ["num_ctx"] = 8192 },
			["messages"] = new JsonArray(
				new JsonObject { ["role"] = "system", ["content"] = instructions },
				new JsonObject
				{
					["role"] = "user",
					["content"] = userMessage,
					["images"] = new JsonArray(Convert.ToBase64String(image)),
				}
			),
		};

	public static RegistrationForm ParseResponse(string responseJson)
	{
		string? content;
		try
		{
			content = JsonNode.Parse(responseJson)?["message"]?["content"]?.GetValue<string>();
		}
		catch (Exception ex) when (ex is JsonException or InvalidOperationException)
		{
			throw new InvalidOperationException("Ollama returned an unexpected response.", ex);
		}

		if (string.IsNullOrWhiteSpace(content))
		{
			throw new InvalidOperationException("Ollama returned no content.");
		}

		try
		{
			return JsonSerializer.Deserialize<RegistrationForm>(content, JsonOptions)
				?? throw new InvalidOperationException("Ollama returned an empty form.");
		}
		catch (JsonException ex)
		{
			throw new InvalidOperationException(
				$"The model did not return a valid registration form: {ex.Message}\n{content}",
				ex
			);
		}
	}

	public async Task<RegistrationForm> ExtractAsync(
		OllamaOptions options,
		byte[] image,
		CancellationToken ct
	)
	{
		var request = BuildRequest(
			options.Model,
			image,
			ExtractionPrompt.Instructions,
			ExtractionPrompt.UserMessage,
			BuildSchema()
		);

		using var response = await _http.PostAsJsonAsync(
			$"{options.BaseUrl}/api/chat",
			request,
			ct
		);
		var body = await response.Content.ReadAsStringAsync(ct);
		if (!response.IsSuccessStatusCode)
		{
			throw new InvalidOperationException(
				$"Ollama returned {(int)response.StatusCode}: {body}"
			);
		}

		return ParseResponse(body);
	}
}
