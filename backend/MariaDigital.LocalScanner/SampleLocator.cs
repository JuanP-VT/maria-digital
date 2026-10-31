namespace MariaDigital.LocalScanner;

public static class SampleLocator
{
	public const string DefaultFileName = "Test.jpeg";

	/// <summary>Looks for the file in the start directory and then in each parent.</summary>
	public static string? FindUpwards(
		string startDirectory,
		string fileName,
		Func<string, bool> fileExists
	)
	{
		var directory = startDirectory;
		while (!string.IsNullOrEmpty(directory))
		{
			var candidate = Path.Combine(directory, fileName);
			if (fileExists(candidate))
			{
				return candidate;
			}

			directory = Path.GetDirectoryName(directory);
		}

		return null;
	}
}
