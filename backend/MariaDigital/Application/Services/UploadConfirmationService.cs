using MariaDigital.Application.DTOs.Requests;
using MariaDigital.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace MariaDigital.Application.Services;

public enum UploadConfirmationStatus
{
	Confirmed,
	Invalid,
	UnknownKey,
}

public sealed class UploadConfirmationService(AppDbContext db)
{
	public static bool IsValid(UploadConfirmedRequest? request) =>
		request is { Key.Length: > 0 and <= 512, Size: >= 0 };

	/// <summary>
	/// Idempotent: the queue delivers at least once, so a repeat of an already confirmed key is
	/// reported as <see cref="UploadConfirmationStatus.Confirmed"/> without touching the row.
	/// </summary>
	public async Task<UploadConfirmationStatus> ConfirmAsync(
		UploadConfirmedRequest? request,
		CancellationToken cancellationToken = default
	)
	{
		if (!IsValid(request))
			return UploadConfirmationStatus.Invalid;

		var document = await db.StudentDocuments.SingleOrDefaultAsync(
			d => d.StorageKey == request!.Key,
			cancellationToken
		);
		if (document is null)
			return UploadConfirmationStatus.UnknownKey;

		document.MarkUploaded(request!.Size, DateTime.UtcNow);
		await db.SaveChangesAsync(cancellationToken);
		return UploadConfirmationStatus.Confirmed;
	}
}
