using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using MariaDigital.Domain.Students;
using MariaDigital.LocalScanner;

namespace MariaDigital.Tests.LocalScanner;

public class RegistrationExtractorTests
{
	private static readonly OllamaOptions Options = new("http://ollama.test", "qwen3.5:9b");

	private const string FormJson = """
		{"level":"","name":"Sindy Guadalupe Garcia Cota","dateOfBirth":"2016-10-31",
		"fathersName":"Jesus Alberto Garcia","healthInstitution":"IMSS",
		"wishToServeInChoir":false,"financialContribution":350}
		""";

	private static string ChatResponse(string content) =>
		new JsonObject { ["message"] = new JsonObject { ["content"] = content } }.ToJsonString();

	private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
	{
		public HttpRequestMessage? Request { get; private set; }
		public string? RequestBody { get; private set; }

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken
		)
		{
			Request = request;
			RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
			return new HttpResponseMessage(status) { Content = new StringContent(body) };
		}
	}

	[Fact]
	public void BuildRequest_DisablesThinkingAndUsesZeroTemperature()
	{
		var request = RegistrationExtractor.BuildRequest(
			"m",
			[1, 2, 3],
			"instructions",
			"go",
			JsonNode.Parse("{}")!
		);

		Assert.False(request["think"]!.GetValue<bool>());
		Assert.False(request["stream"]!.GetValue<bool>());
		Assert.Equal(0, request["options"]!["temperature"]!.GetValue<int>());
	}

	[Fact]
	public void BuildRequest_SendsInstructionsAndTheImage()
	{
		var request = RegistrationExtractor.BuildRequest(
			"m",
			[1, 2, 3],
			"instructions",
			"go",
			JsonNode.Parse("{}")!
		);

		var messages = request["messages"]!.AsArray();
		Assert.Equal("m", request["model"]!.GetValue<string>());
		Assert.Equal("system", messages[0]!["role"]!.GetValue<string>());
		Assert.Equal("instructions", messages[0]!["content"]!.GetValue<string>());
		Assert.Equal("user", messages[1]!["role"]!.GetValue<string>());
		Assert.Equal(
			Convert.ToBase64String([1, 2, 3]),
			messages[1]!["images"]![0]!.GetValue<string>()
		);
	}

	[Fact]
	public void BuildSchema_DescribesTheRegistrationForm()
	{
		var properties = RegistrationExtractor.BuildSchema()["properties"]!.AsObject();

		Assert.True(properties.ContainsKey("name"));
		Assert.True(properties.ContainsKey("dateOfBirth"));
		Assert.True(properties.ContainsKey("healthInstitution"));
		Assert.True(properties.ContainsKey("financialContribution"));
	}

	[Fact]
	public void ParseResponse_MapsFieldsAndStringEnums()
	{
		var form = RegistrationExtractor.ParseResponse(ChatResponse(FormJson));

		Assert.Equal("Sindy Guadalupe Garcia Cota", form.Name);
		Assert.Equal(new DateTime(2016, 10, 31), form.DateOfBirth);
		Assert.Equal(MedicalHealthInstitution.IMSS, form.HealthInstitution);
		Assert.Equal(350m, form.FinancialContribution);
		Assert.Null(form.Email);
	}

	[Theory]
	[InlineData("not json")]
	[InlineData("{}")]
	[InlineData("""{"message":{"content":""}}""")]
	public void ParseResponse_WithoutContent_Throws(string response)
	{
		Assert.Throws<InvalidOperationException>(() =>
			RegistrationExtractor.ParseResponse(response)
		);
	}

	[Fact]
	public void ParseResponse_InvalidFormJson_ThrowsAndIncludesTheModelOutput()
	{
		var ex = Assert.Throws<InvalidOperationException>(() =>
			RegistrationExtractor.ParseResponse(ChatResponse("""{"dateOfBirth":"mañana"}"""))
		);

		Assert.Contains("mañana", ex.Message);
	}

	[Fact]
	public async Task ExtractAsync_PostsToChatEndpointAndReturnsTheForm()
	{
		var handler = new StubHandler(HttpStatusCode.OK, ChatResponse(FormJson));
		var extractor = new RegistrationExtractor(new HttpClient(handler));

		var form = await extractor.ExtractAsync(Options, [1], CancellationToken.None);

		Assert.Equal("http://ollama.test/api/chat", handler.Request!.RequestUri!.ToString());
		Assert.Equal(HttpMethod.Post, handler.Request.Method);
		Assert.Equal(
			"qwen3.5:9b",
			JsonNode.Parse(handler.RequestBody!)!["model"]!.GetValue<string>()
		);
		Assert.Equal("Sindy Guadalupe Garcia Cota", form.Name);
	}

	[Fact]
	public async Task ExtractAsync_ErrorStatus_ThrowsWithOllamaMessage()
	{
		var handler = new StubHandler(
			HttpStatusCode.BadRequest,
			"""{"error":"model does not support images"}"""
		);
		var extractor = new RegistrationExtractor(new HttpClient(handler));

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
			extractor.ExtractAsync(Options, [1], CancellationToken.None)
		);

		Assert.Contains("model does not support images", ex.Message);
	}
}
