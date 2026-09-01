namespace GuadalupeDigital.Application.Abstractions.ObjectStorage;

public sealed record StoredObject(
    string Key,
    string? Etag,
    long Size);
