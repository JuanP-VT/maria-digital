namespace MariaDigital.Application.DTOs.Responses;

public sealed record PresignUploadsResponse(
	Guid SubmissionId,
	IReadOnlyList<PresignedFile> Uploads
);

/// <param name="Headers">Send these exactly with the PUT; Content-Length comes from the body.</param>
public sealed record PresignedFile(
	string Type,
	string Url,
	IReadOnlyDictionary<string, string> Headers,
	DateTime ExpiresAt
);
