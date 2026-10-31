using System.Text.Json;

namespace MariaDigital.LocalScanner;

public static class OllamaTags
{
	public static bool IsModelInstalled(string tagsJson, string model)
	{
		try
		{
			using var document = JsonDocument.Parse(tagsJson);
			if (!document.RootElement.TryGetProperty("models", out var models))
			{
				return false;
			}

			return models
				.EnumerateArray()
				.Select(m => m.TryGetProperty("name", out var name) ? name.GetString() : null)
				.Any(name => Matches(name, model));
		}
		catch (JsonException)
		{
			return false;
		}
	}

	private static bool Matches(string? installed, string requested)
	{
		if (installed is null)
		{
			return false;
		}

		var wanted = requested.Contains(':') ? requested : $"{requested}:latest";
		return string.Equals(installed, wanted, StringComparison.OrdinalIgnoreCase);
	}
}
