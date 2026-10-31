namespace MariaDigital.QueuePoller;

/// <summary>Port of worker/src/process.ts: turns one queue message into an ack or a retry.</summary>
public sealed class MessageProcessor(
    IUploadConfirmer confirmer,
    Func<double> random,
    Action<string> log
)
{
    public async Task<MessageOutcome> ProcessAsync(QueueMessage message, CancellationToken ct)
    {
        var parsed = MessageParser.Parse(message.Body);
        switch (parsed.Kind)
        {
            case MessageKind.Ignore:
                return new MessageOutcome.Ack();
            case MessageKind.Invalid:
                log($"Invalid message dropped: {message.Body.GetRawText()}");
                return new MessageOutcome.Ack();
        }

        try
        {
            if (await confirmer.ConfirmAsync(parsed.Key, parsed.Size, ct))
            {
                log($"Confirmed {parsed.Key} ({parsed.Size} bytes).");
                return new MessageOutcome.Ack();
            }

            log($"Backend rejected {parsed.Key}; will retry.");
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            if (ct.IsCancellationRequested)
                throw;
            log($"Webhook failed for {parsed.Key}: {ex.Message}");
        }

        return new MessageOutcome.Retry(RetryPolicy.DelaySeconds(message.Attempts, random));
    }
}
