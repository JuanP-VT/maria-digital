namespace MariaDigital.QueuePoller;

/// <summary>Port of worker/src/retry.ts: exponential backoff with jitter.</summary>
public static class RetryPolicy
{
    private const int BaseSeconds = 5;
    private const int CapSeconds = 600;

    public static int DelaySeconds(int attempts, Func<double> random)
    {
        var step = Math.Max(0, attempts - 1);
        var delay = Math.Min((double)CapSeconds, BaseSeconds * Math.Pow(3, step));
        return Math.Max(
            1,
            (int)Math.Round(delay * (0.5 + random() * 0.5), MidpointRounding.AwayFromZero)
        );
    }
}
