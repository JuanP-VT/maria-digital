namespace MariaDigital.LocalScanner;

public record OllamaOptions(string BaseUrl, string Model)
{
    public const string DefaultBaseUrl = "http://localhost:11434";

    public static OllamaOptions Resolve(string[] args, Func<string, string?> getEnv)
    {
        var model =
            args.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a)) ?? getEnv("OLLAMA_MODEL");
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException(
                "Model name required: pass it as the first argument or set OLLAMA_MODEL (see `ollama list`)."
            );
        }

        var baseUrl = getEnv("OLLAMA_URL");
        return new OllamaOptions(
            string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUrl : baseUrl.Trim().TrimEnd('/'),
            model.Trim()
        );
    }
}
