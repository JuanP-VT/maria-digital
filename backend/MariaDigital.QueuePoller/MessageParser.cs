using System.Text.Json;

namespace MariaDigital.QueuePoller;

public enum MessageKind
{
    Confirm,
    Ignore,
    Invalid,
}

public sealed record ParsedMessage(MessageKind Kind, string Key = "", long Size = 0);

/// <summary>Port of worker/src/message.ts: decides what to do with an R2 event notification.</summary>
public static class MessageParser
{
    public const string KeyPrefix = "submissions/";
    private const long MaxSafeInteger = 9_007_199_254_740_991;

    private static readonly HashSet<string> CreateActions =
    [
        "PutObject",
        "CopyObject",
        "CompleteMultipartUpload",
    ];

    public static ParsedMessage Parse(JsonElement body)
    {
        var invalid = new ParsedMessage(MessageKind.Invalid);
        if (body.ValueKind != JsonValueKind.Object)
            return invalid;

        if (
            !body.TryGetProperty("action", out var action)
            || action.ValueKind != JsonValueKind.String
        )
            return invalid;

        if (
            !body.TryGetProperty("object", out var obj)
            || obj.ValueKind != JsonValueKind.Object
            || !obj.TryGetProperty("key", out var keyElement)
            || keyElement.ValueKind != JsonValueKind.String
        )
            return invalid;

        var key = keyElement.GetString();
        if (string.IsNullOrEmpty(key))
            return invalid;

        if (
            !obj.TryGetProperty("size", out var sizeElement)
            || sizeElement.ValueKind != JsonValueKind.Number
            || !sizeElement.TryGetInt64(out var size)
            || size < 0
            || size > MaxSafeInteger
        )
            return invalid;

        if (!CreateActions.Contains(action.GetString()!))
            return new ParsedMessage(MessageKind.Ignore);

        if (!key.StartsWith(KeyPrefix, StringComparison.Ordinal))
            return new ParsedMessage(MessageKind.Ignore);

        return new ParsedMessage(MessageKind.Confirm, key, size);
    }
}
