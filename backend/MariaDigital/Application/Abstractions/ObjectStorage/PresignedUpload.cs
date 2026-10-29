namespace MariaDigital.Application.Abstractions.ObjectStorage;

public sealed record PresignedUpload(Uri Url, IDictionary<string, string> FormFields);
