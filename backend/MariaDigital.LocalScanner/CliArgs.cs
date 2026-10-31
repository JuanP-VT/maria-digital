namespace MariaDigital.LocalScanner;

/// <summary>`&lt;model&gt; [--image path] [--chat]`: the first bare argument is the model.</summary>
public record CliArgs(string? Model, string? ImagePath, bool Chat)
{
	public static CliArgs Parse(string[] args)
	{
		string? model = null;
		string? imagePath = null;
		var chat = false;

		for (var i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--chat":
					chat = true;
					break;
				case "--image":
					imagePath = i + 1 < args.Length ? args[++i] : null;
					break;
				default:
					model ??= args[i];
					break;
			}
		}

		return new CliArgs(model, imagePath, chat);
	}
}
