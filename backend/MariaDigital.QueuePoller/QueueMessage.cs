using System.Text.Json;

namespace MariaDigital.QueuePoller;

public sealed record QueueMessage(string LeaseId, int Attempts, JsonElement Body);

public abstract record MessageOutcome
{
    public sealed record Ack : MessageOutcome;

    public sealed record Retry(int DelaySeconds) : MessageOutcome;
}

public sealed record SettledMessage(QueueMessage Message, MessageOutcome Outcome);
