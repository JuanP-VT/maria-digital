namespace MariaDigital.Application.DTOs.Requests;

public sealed record PresignUploadsRequest(IReadOnlyList<PresignFileRequest>? Files);

/// <param name="Type">Wizard step id: registration, birth, baptism or address.</param>
public sealed record PresignFileRequest(string? Type, string? ContentType, long SizeBytes);
