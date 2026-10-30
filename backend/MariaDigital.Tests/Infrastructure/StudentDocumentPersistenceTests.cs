using MariaDigital.Domain.Students;
using MariaDigital.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MariaDigital.Tests.Infrastructure;

public sealed class StudentDocumentPersistenceTests : IDisposable
{
	private readonly TestApp _app = new();

	public void Dispose() => _app.Dispose();

	private AppDbContext NewDb() =>
		_app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

	[Fact]
	public async Task A_submission_can_hold_one_document_of_each_type_before_any_student_exists()
	{
		var submission = Guid.NewGuid();
		using (var db = NewDb())
		{
			foreach (var type in Enum.GetValues<StudentDocumentType>())
				db.StudentDocuments.Add(
					StudentDocument.CreatePending(submission, type, "image/jpeg")
				);
			await db.SaveChangesAsync();
		}

		using var read = NewDb();
		var docs = await read
			.StudentDocuments.Where(d => d.SubmissionId == submission)
			.ToListAsync();
		Assert.Equal(4, docs.Count);
		Assert.All(docs, d => Assert.Null(d.StudentId));
	}

	[Fact]
	public async Task Two_documents_of_the_same_type_in_one_submission_are_rejected()
	{
		var submission = Guid.NewGuid();
		using var db = NewDb();
		db.StudentDocuments.Add(
			StudentDocument.CreatePending(submission, StudentDocumentType.Birth, "image/jpeg")
		);
		db.StudentDocuments.Add(
			StudentDocument.CreatePending(submission, StudentDocumentType.Birth, "image/jpeg")
		);

		await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
	}

	[Fact]
	public async Task Attaching_to_a_student_and_marking_uploaded_persist()
	{
		var submission = Guid.NewGuid();
		long studentId;
		string key;
		using (var db = NewDb())
		{
			var student = new Student { Name = "Ana" };
			var doc = StudentDocument.CreatePending(
				submission,
				StudentDocumentType.Registration,
				"image/jpeg"
			);
			key = doc.StorageKey;
			db.Students.Add(student);
			db.StudentDocuments.Add(doc);
			await db.SaveChangesAsync();
			studentId = student.Id;
		}

		using (var db = NewDb())
		{
			var doc = await db.StudentDocuments.SingleAsync(d => d.StorageKey == key);
			doc.MarkUploaded(500, DateTime.UtcNow);
			doc.AttachToStudent(studentId);
			await db.SaveChangesAsync();
		}

		using var read = NewDb();
		var saved = await read.StudentDocuments.SingleAsync(d => d.StorageKey == key);
		Assert.Equal(DocumentStatus.Uploaded, saved.Status);
		Assert.Equal(500, saved.SizeBytes);
		Assert.Equal(studentId, saved.StudentId);
	}
}
