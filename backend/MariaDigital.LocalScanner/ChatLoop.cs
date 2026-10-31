using System.Text;

namespace MariaDigital.LocalScanner;

public record ChatTurn(bool IsUser, string Text);

public class ChatLoop
{
	private readonly Func<
		IReadOnlyList<ChatTurn>,
		CancellationToken,
		IAsyncEnumerable<string>
	> _reply;
	private readonly TextReader _input;
	private readonly TextWriter _output;

	public ChatLoop(
		Func<IReadOnlyList<ChatTurn>, CancellationToken, IAsyncEnumerable<string>> reply,
		TextReader input,
		TextWriter output
	)
	{
		_reply = reply;
		_input = input;
		_output = output;
	}

	/// <summary>Reads lines until "exit" or end of input, streaming each answer to the output.</summary>
	public async Task RunAsync(CancellationToken ct)
	{
		var history = new List<ChatTurn>();
		while (true)
		{
			await _output.WriteAsync("> ");
			var line = await _input.ReadLineAsync(ct);
			if (line is null || line.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}

			history.Add(new ChatTurn(true, line));
			var answer = new StringBuilder();
			await foreach (var chunk in _reply([.. history], ct))
			{
				answer.Append(chunk);
				await _output.WriteAsync(chunk);
			}

			await _output.WriteLineAsync();
			history.Add(new ChatTurn(false, answer.ToString()));
		}
	}
}
