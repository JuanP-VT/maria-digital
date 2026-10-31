using System.Diagnostics;
using System.Text.Json;

namespace MariaDigital.LocalScanner;

internal static class Program
{
	private static async Task<int> Main(string[] args)
	{
		OllamaOptions options;
		try
		{
			options = OllamaOptions.Resolve(args, Environment.GetEnvironmentVariable);
		}
		catch (ArgumentException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return 1;
		}

		var cli = CliArgs.Parse(args);
		using var probe = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
		var bootstrapper = new OllamaBootstrapper(
			probe,
			OllamaServerProcess.Start,
			startupTimeout: TimeSpan.FromSeconds(30),
			pollInterval: TimeSpan.FromMilliseconds(500)
		);

		IDisposable server;
		try
		{
			Console.WriteLine($"Checking Ollama at {options.BaseUrl}...");
			server = await bootstrapper.EnsureReadyAsync(options, CancellationToken.None);
		}
		catch (InvalidOperationException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return 1;
		}

		using (server)
		{
			Console.CancelKeyPress += (_, _) => server.Dispose();
			Console.WriteLine($"Ready: {options.Model}.");

			if (cli.Chat)
			{
				return await RunChatAsync(options);
			}

			return await RunScanAsync(options, cli.ImagePath);
		}
	}

	private static async Task<int> RunChatAsync(OllamaOptions options)
	{
		Console.WriteLine("Type a message, or 'exit' to quit.");
		var chat = new OllamaChat(options);
		var loop = new ChatLoop(chat.StreamReplyAsync, Console.In, Console.Out);
		await loop.RunAsync(CancellationToken.None);
		return 0;
	}

	private static async Task<int> RunScanAsync(OllamaOptions options, string? imagePath)
	{
		imagePath ??= SampleLocator.FindUpwards(
			Directory.GetCurrentDirectory(),
			SampleLocator.DefaultFileName,
			File.Exists
		);
		if (imagePath is null || !File.Exists(imagePath))
		{
			Console.Error.WriteLine(
				$"Sheet photo not found. Pass --image <path> or place {SampleLocator.DefaultFileName} in the repo root."
			);
			return 1;
		}

		Console.WriteLine($"Scanning {imagePath} (the first run also loads the model)...");
		var image = await File.ReadAllBytesAsync(imagePath);
		using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
		var extractor = new RegistrationExtractor(http);

		try
		{
			var timer = Stopwatch.StartNew();
			var form = await extractor.ExtractAsync(options, image, CancellationToken.None);
			Console.WriteLine($"Done in {timer.Elapsed.TotalSeconds:0.0}s:");
			Console.WriteLine(
				JsonSerializer.Serialize(
					form,
					new JsonSerializerOptions(RegistrationExtractor.JsonOptions)
					{
						WriteIndented = true,
					}
				)
			);
			return 0;
		}
		catch (InvalidOperationException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return 1;
		}
	}
}
