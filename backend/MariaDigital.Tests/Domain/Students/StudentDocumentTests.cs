using MariaDigital.Domain.Students;

namespace MariaDigital.Tests.Domain.Students;

public class StudentDocumentTests
{
	private static readonly Guid Submission = Guid.NewGuid();

	[Fact]
	public void CreatePending_starts_pending_without_student_or_size()
	{
		var doc = StudentDocument.CreatePending(
			Submission,
			StudentDocumentType.Birth,
			"image/jpeg"
		);

		Assert.Equal(DocumentStatus.Pending, doc.Status);
		Assert.Equal(Submission, doc.SubmissionId);
		Assert.Equal(StudentDocumentType.Birth, doc.Type);
		Assert.Equal("image/jpeg", doc.ContentType);
		Assert.Null(doc.StudentId);
		Assert.Null(doc.SizeBytes);
		Assert.Null(doc.UploadedAt);
	}

	[Fact]
	public void BuildStorageKey_is_scoped_by_submission_and_type_and_unique_per_call()
	{
		var first = StudentDocument.BuildStorageKey(Submission, StudentDocumentType.Baptism);
		var second = StudentDocument.BuildStorageKey(Submission, StudentDocumentType.Baptism);

		Assert.StartsWith($"submissions/{Submission:N}/baptism/", first);
		Assert.NotEqual(first, second);
	}

	[Fact]
	public void MarkUploaded_sets_status_size_and_timestamp()
	{
		var doc = StudentDocument.CreatePending(
			Submission,
			StudentDocumentType.Address,
			"image/png"
		);
		var now = new DateTime(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);

		doc.MarkUploaded(1234, now);

		Assert.Equal(DocumentStatus.Uploaded, doc.Status);
		Assert.Equal(1234, doc.SizeBytes);
		Assert.Equal(now, doc.UploadedAt);
	}

	[Fact]
	public void MarkUploaded_is_idempotent_when_the_webhook_is_redelivered()
	{
		var doc = StudentDocument.CreatePending(
			Submission,
			StudentDocumentType.Registration,
			"image/jpeg"
		);
		var firstAt = new DateTime(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);

		doc.MarkUploaded(100, firstAt);
		doc.MarkUploaded(999, firstAt.AddMinutes(5));

		Assert.Equal(100, doc.SizeBytes);
		Assert.Equal(firstAt, doc.UploadedAt);
	}

	[Fact]
	public void AttachToStudent_sets_the_student()
	{
		var doc = StudentDocument.CreatePending(
			Submission,
			StudentDocumentType.Registration,
			"image/jpeg"
		);

		doc.AttachToStudent(42);

		Assert.Equal(42, doc.StudentId);
	}
}
