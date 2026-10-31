using System.ClientModel;
using System.Runtime.CompilerServices;
using OpenAI;
using OpenAI.Chat;

namespace MariaDigital.LocalScanner;

/// <summary>Streams chat answers from Ollama through its OpenAI-compatible endpoint.</summary>
public class OllamaChat
{
	private readonly ChatClient _client;

	public OllamaChat(OllamaOptions options)
	{
		_client = new ChatClient(
			options.Model,
			new ApiKeyCredential("ollama"),
			new OpenAIClientOptions
			{
				Endpoint = new Uri($"{options.BaseUrl}/v1"),
				NetworkTimeout = TimeSpan.FromMinutes(5),
			}
		);
	}

	public async IAsyncEnumerable<string> StreamReplyAsync(
		IReadOnlyList<ChatTurn> history,
		[EnumeratorCancellation] CancellationToken ct
	)
	{
		var messages = history.Select(t =>
			t.IsUser ? (ChatMessage)new UserChatMessage(t.Text) : new AssistantChatMessage(t.Text)
		);

		await foreach (
			var update in _client.CompleteChatStreamingAsync(messages, cancellationToken: ct)
		)
		{
			foreach (var part in update.ContentUpdate)
			{
				yield return part.Text;
			}
		}
	}
}
