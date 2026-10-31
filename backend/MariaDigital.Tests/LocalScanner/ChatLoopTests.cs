using MariaDigital.LocalScanner;

namespace MariaDigital.Tests.LocalScanner;

public class ChatLoopTests
{
	private static async IAsyncEnumerable<string> Chunks(params string[] chunks)
	{
		foreach (var chunk in chunks)
		{
			yield return chunk;
		}

		await Task.CompletedTask;
	}

	private static async Task<(string Output, List<IReadOnlyList<ChatTurn>> Calls)> Run(
		string input
	)
	{
		var calls = new List<IReadOnlyList<ChatTurn>>();
		var output = new StringWriter();
		var loop = new ChatLoop(
			(history, _) =>
			{
				calls.Add(history);
				return Chunks("Soy ", $"respuesta {calls.Count}");
			},
			new StringReader(input),
			output
		);

		await loop.RunAsync(CancellationToken.None);

		return (output.ToString(), calls);
	}

	[Fact]
	public async Task StreamsTheAnswerToTheOutput()
	{
		var (output, _) = await Run("hola\nexit\n");

		Assert.Contains("Soy respuesta 1", output);
	}

	[Fact]
	public async Task ExitStopsBeforeReadingMoreLines()
	{
		var (_, calls) = await Run("exit\nhola\n");

		Assert.Empty(calls);
	}

	[Fact]
	public async Task ExitIsCaseInsensitive()
	{
		var (_, calls) = await Run("  EXIT  \nhola\n");

		Assert.Empty(calls);
	}

	[Fact]
	public async Task EndOfInputStopsTheLoop()
	{
		var (_, calls) = await Run("hola");

		Assert.Single(calls);
	}

	[Fact]
	public async Task BlankLinesAreIgnored()
	{
		var (_, calls) = await Run("\n   \nhola\n");

		Assert.Single(calls);
	}

	[Fact]
	public async Task HistoryCarriesPreviousTurns()
	{
		var (_, calls) = await Run("uno\ndos\n");

		Assert.Equal(2, calls.Count);
		Assert.Equal(
			[
				new ChatTurn(true, "uno"),
				new ChatTurn(false, "Soy respuesta 1"),
				new ChatTurn(true, "dos"),
			],
			calls[1]
		);
	}
}
