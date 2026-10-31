using MariaDigital.LocalScanner;

namespace MariaDigital.Tests.LocalScanner;

public class CliArgsTests
{
	[Fact]
	public void Parse_NoArgs_HasNoModelImageOrChat()
	{
		Assert.Equal(new CliArgs(null, null, false), CliArgs.Parse([]));
	}

	[Fact]
	public void Parse_FirstBareArgumentIsTheModel()
	{
		Assert.Equal("qwen3.5:9b", CliArgs.Parse(["qwen3.5:9b", "other"]).Model);
	}

	[Fact]
	public void Parse_ImageFlagTakesTheNextValue()
	{
		var args = CliArgs.Parse(["--image", "a.jpg", "qwen3.5:9b"]);

		Assert.Equal("a.jpg", args.ImagePath);
		Assert.Equal("qwen3.5:9b", args.Model);
	}

	[Fact]
	public void Parse_ImageFlagWithoutValue_IsNull()
	{
		Assert.Null(CliArgs.Parse(["m", "--image"]).ImagePath);
	}

	[Fact]
	public void Parse_ChatFlag()
	{
		var args = CliArgs.Parse(["m", "--chat"]);

		Assert.True(args.Chat);
		Assert.Equal("m", args.Model);
	}
}
