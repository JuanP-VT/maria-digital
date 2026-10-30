namespace MariaDigital.Domain.Students;

/// <summary>
/// Links one file in object storage to a registration submission. The row is created as
/// <see cref="DocumentStatus.Pending"/> when the presigned upload URL is issued and flipped to
/// <see cref="DocumentStatus.Uploaded"/> when the storage webhook confirms the object exists.
/// The student does not exist yet at upload time: a later job reads the registration sheet,
/// creates the student and attaches the submission's documents to it.
/// </summary>
public class StudentDocument
{
	private StudentDocument() { }

	public long Id { get; private set; }

	/// <summary>Groups the files uploaded together (one per <see cref="StudentDocumentType"/>).</summary>
	public Guid SubmissionId { get; private set; }
	public long? StudentId { get; private set; }
	public Student? Student { get; private set; }
	public StudentDocumentType Type { get; private set; }
	public string StorageKey { get; private set; } = string.Empty;
	public string ContentType { get; private set; } = string.Empty;
	public long? SizeBytes { get; private set; }
	public DocumentStatus Status { get; private set; } = DocumentStatus.Pending;
	public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
	public DateTime? UploadedAt { get; private set; }

	public static StudentDocument CreatePending(
		Guid submissionId,
		StudentDocumentType type,
		string contentType
	) =>
		new()
		{
			SubmissionId = submissionId,
			Type = type,
			ContentType = contentType,
			StorageKey = BuildStorageKey(submissionId, type),
		};

	/// <summary>Idempotent: storage webhooks can be delivered more than once.</summary>
	public void MarkUploaded(long sizeBytes, DateTime now)
	{
		if (Status == DocumentStatus.Uploaded)
			return;

		Status = DocumentStatus.Uploaded;
		SizeBytes = sizeBytes;
		UploadedAt = now;
	}

	public void AttachToStudent(long studentId)
	{
		StudentId = studentId;
	}

	public static string BuildStorageKey(Guid submissionId, StudentDocumentType type) =>
		$"submissions/{submissionId:N}/{type.ToString().ToLowerInvariant()}/{Guid.NewGuid():N}";
}

// Mirrors the wizard step ids in frontend/src/components/doc-wizard/config.ts.
public enum StudentDocumentType
{
	Registration = 0,
	Birth = 1,
	Baptism = 2,
	Address = 3,
}

public enum DocumentStatus
{
	Pending = 0,
	Uploaded = 1,
}
