using MariaDigital.LocalScanner;

namespace MariaDigital.Tests.LocalScanner;

public class OllamaTagsTests
{
	private const string Tags = """{"models":[{"name":"qwen3.5:9b"},{"name":"gemma3:12b"}]}""";

	[Fact]
	public void IsModelInstalled_ExactTag_ReturnsTrue()
	{
		Assert.True(OllamaTags.IsModelInstalled(Tags, "qwen3.5:9b"));
	}

	[Fact]
	public void IsModelInstalled_IsCaseInsensitive()
	{
		Assert.True(OllamaTags.IsModelInstalled(Tags, "Gemma3:12B"));
	}

	[Fact]
	public void IsModelInstalled_BareNameMatchesLatestTag()
	{
		var tags = """{"models":[{"name":"llava:latest"}]}""";

		Assert.True(OllamaTags.IsModelInstalled(tags, "llava"));
	}

	[Fact]
	public void IsModelInstalled_BareNameDoesNotMatchOtherTag()
	{
		Assert.False(OllamaTags.IsModelInstalled(Tags, "qwen3.5"));
	}

	[Fact]
	public void IsModelInstalled_MissingModel_ReturnsFalse()
	{
		Assert.False(OllamaTags.IsModelInstalled(Tags, "qwen3.5:27b"));
	}

	[Theory]
	[InlineData("not json")]
	[InlineData("{}")]
	[InlineData("""{"models":[{}]}""")]
	public void IsModelInstalled_UnexpectedPayload_ReturnsFalse(string json)
	{
		Assert.False(OllamaTags.IsModelInstalled(json, "qwen3.5:9b"));
	}
}
