namespace MariaDigital.QueuePoller;

/// <summary>Pull a batch, settle each message concurrently, ack/retry them in one call.</summary>
public sealed class PollLoop(
    IQueueClient queue,
    MessageProcessor processor,
    TimeSpan pollInterval,
    Action<string> log
)
{
    /// <summary>One pull-process-ack round; returns how many messages were handled.</summary>
    public async Task<int> RunOnceAsync(CancellationToken ct)
    {
        var messages = await queue.PullAsync(ct);
        if (messages.Count == 0)
            return 0;

        var settled = await Task.WhenAll(
            messages.Select(async m => new SettledMessage(m, await processor.ProcessAsync(m, ct)))
        );
        await queue.AckAsync(settled, ct);
        return messages.Count;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var handled = 0;
            try
            {
                handled = await RunOnceAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException)
            {
                log($"Poll failed: {ex.Message}");
            }

            if (handled > 0)
                continue;

            try
            {
                await Task.Delay(pollInterval, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
