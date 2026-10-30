using MariaDigital.Application.DTOs.Requests;
using MariaDigital.Application.DTOs.Responses;
using MariaDigital.Domain.Students;
using MariaDigital.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace MariaDigital.Application.Services;

public enum PresignStatus
{
	Ok,
	Invalid,
	TooManyPending,
}

public sealed record PresignOutcome(
	PresignStatus Status,
	PresignUploadsResponse? Response = null,
	IReadOnlyList<string>? Errors = null
);

public sealed class PresignedUploadService(AppDbContext db, IObjectStorage storage)
{
	public static readonly TimeSpan UrlLifetime = TimeSpan.FromMinutes(5);

	/// <summary>Bounds how many unconfirmed rows (and so bucket objects) can pile up per hour.</summary>
	public const int MaxPendingPerHour = 40;

	public async Task<PresignOutcome> CreateAsync(
		PresignUploadsRequest? request,
		CancellationToken cancellationToken = default
	)
	{
		var errors = UploadRequestValidator.Validate(request);
		if (errors.Count > 0)
			return new PresignOutcome(PresignStatus.Invalid, Errors: errors);

		var files = request!.Files!;
		var since = DateTime.UtcNow.AddHours(-1);
		var recentPending = await db.StudentDocuments.CountAsync(
			d => d.Status == DocumentStatus.Pending && d.CreatedAt >= since,
			cancellationToken
		);
		if (recentPending + files.Count > MaxPendingPerHour)
			return new PresignOutcome(PresignStatus.TooManyPending);

		var submissionId = Guid.NewGuid();
		var rows = files
			.Select(file =>
			{
				UploadRequestValidator.TryParseType(file.Type, out var type);
				return (
					Row: StudentDocument.CreatePending(
						submissionId,
						type,
						UploadRequestValidator.NormalizeContentType(file.ContentType)
					),
					file.SizeBytes
				);
			})
			.ToList();

		// Rows first: a URL must never exist for a key the database does not know about.
		db.StudentDocuments.AddRange(rows.Select(r => r.Row));
		await db.SaveChangesAsync(cancellationToken);

		var expiresAt = DateTime.UtcNow.Add(UrlLifetime);
		var uploads = new List<PresignedFile>();
		foreach (var (row, sizeBytes) in rows)
		{
			var url = await storage.GetPresignedPutAsync(
				row.StorageKey,
				row.ContentType,
				sizeBytes,
				UrlLifetime,
				cancellationToken
			);
			uploads.Add(
				new PresignedFile(
					row.Type.ToString().ToLowerInvariant(),
					url,
					new Dictionary<string, string> { ["Content-Type"] = row.ContentType },
					expiresAt
				)
			);
		}

		return new PresignOutcome(
			PresignStatus.Ok,
			new PresignUploadsResponse(submissionId, uploads)
		);
	}
}
