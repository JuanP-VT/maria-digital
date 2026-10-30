namespace MariaDigital.Application.DTOs.Requests;

/// <summary>Sent by the Cloudflare Worker when R2 reports that an object was created.</summary>
public sealed record UploadConfirmedRequest(string? Key, long Size);
