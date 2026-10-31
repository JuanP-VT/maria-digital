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

		using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
		var bootstrapper = new OllamaBootstrapper(
			http,
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
			Console.WriteLine($"Ready: {options.Model}. Type a message, or 'exit' to quit.");

			var chat = new OllamaChat(options);
			var loop = new ChatLoop(chat.StreamReplyAsync, Console.In, Console.Out);
			await loop.RunAsync(CancellationToken.None);
		}

		return 0;
	}
}
