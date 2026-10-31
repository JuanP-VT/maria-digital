using MariaDigital.LocalScanner;

namespace MariaDigital.Tests.LocalScanner;

public class OllamaOptionsTests
{
	private static Func<string, string?> Env(params (string Key, string Value)[] values) =>
		key => values.Where(v => v.Key == key).Select(v => v.Value).FirstOrDefault();

	[Fact]
	public void Resolve_PrefersArgumentOverEnvironment()
	{
		var options = OllamaOptions.Resolve(["from-arg"], Env(("OLLAMA_MODEL", "from-env")));

		Assert.Equal("from-arg", options.Model);
	}

	[Fact]
	public void Resolve_FallsBackToEnvironmentModel()
	{
		var options = OllamaOptions.Resolve([], Env(("OLLAMA_MODEL", "from-env")));

		Assert.Equal("from-env", options.Model);
	}

	[Fact]
	public void Resolve_WithoutModel_Throws()
	{
		Assert.Throws<ArgumentException>(() => OllamaOptions.Resolve([" "], Env()));
	}

	[Fact]
	public void Resolve_UsesDefaultBaseUrl()
	{
		var options = OllamaOptions.Resolve(["m"], Env());

		Assert.Equal(OllamaOptions.DefaultBaseUrl, options.BaseUrl);
	}

	[Fact]
	public void Resolve_TrimsTrailingSlashFromConfiguredUrl()
	{
		var options = OllamaOptions.Resolve(["m"], Env(("OLLAMA_URL", "http://host:1234/")));

		Assert.Equal("http://host:1234", options.BaseUrl);
	}
}
