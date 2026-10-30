using MariaDigital.Application.DTOs.Requests;
using MariaDigital.Application.Services;
using MariaDigital.Domain.Students;
using MariaDigital.Infrastructure.Persistance;
using MariaDigital.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MariaDigital.Tests.Application.Services;

public sealed class PresignedUploadServiceTests : IDisposable
{
	private readonly TestApp _app = new();
	private readonly IServiceScope _scope;
	private readonly AppDbContext _db;
	private readonly PresignedUploadService _service;

	public PresignedUploadServiceTests()
	{
		_scope = _app.Services.CreateScope();
		_db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
		_service = new PresignedUploadService(_db, _app.Storage);
	}

	public void Dispose()
	{
		_scope.Dispose();
		_app.Dispose();
	}

	private static PresignUploadsRequest Request(
		params (string Type, string ContentType, long Size)[] files
	) => new(files.Select(f => new PresignFileRequest(f.Type, f.ContentType, f.Size)).ToList());

	[Fact]
	public async Task Creates_pending_rows_under_one_submission_and_presigns_each_key()
	{
		var outcome = await _service.CreateAsync(
			Request(("registration", "image/jpeg", 1000), ("birth", "application/pdf", 2000))
		);

		Assert.Equal(PresignStatus.Ok, outcome.Status);
		var response = outcome.Response!;
		Assert.Equal(2, response.Uploads.Count);

		var rows = await _db.StudentDocuments.ToListAsync();
		Assert.Equal(2, rows.Count);
		Assert.All(
			rows,
			r =>
			{
				Assert.Equal(DocumentStatus.Pending, r.Status);
				Assert.Equal(response.SubmissionId, r.SubmissionId);
				Assert.Null(r.StudentId);
			}
		);
		Assert.Equal(
			rows.Select(r => r.StorageKey).Order(),
			_app.Storage.PresignCalls.Select(c => c.Key).Order()
		);
	}

	[Fact]
	public async Task Passes_the_declared_type_size_and_a_short_expiry_to_storage()
	{
		await _service.CreateAsync(Request(("baptism", "Image/PNG", 4321)));

		var call = Assert.Single(_app.Storage.PresignCalls);
		Assert.Equal("image/png", call.ContentType);
		Assert.Equal(4321, call.SizeBytes);
		Assert.Equal(PresignedUploadService.UrlLifetime, call.ExpiresIn);
		Assert.True(call.ExpiresIn <= TimeSpan.FromMinutes(10));
	}

	[Fact]
	public async Task Response_tells_the_client_which_header_to_send_and_never_echoes_a_client_key()
	{
		var outcome = await _service.CreateAsync(Request(("address", "image/jpeg", 10)));

		var upload = Assert.Single(outcome.Response!.Uploads);
		Assert.Equal("address", upload.Type);
		Assert.Equal("image/jpeg", upload.Headers["Content-Type"]);
		Assert.StartsWith(
			$"https://storage.test/submissions/{outcome.Response.SubmissionId:N}/address/",
			upload.Url
		);
	}

	[Fact]
	public async Task Invalid_input_saves_nothing_and_presigns_nothing()
	{
		var outcome = await _service.CreateAsync(
			Request(("birth", "image/jpeg", 10), ("baptism", "text/html", 10))
		);

		Assert.Equal(PresignStatus.Invalid, outcome.Status);
		Assert.NotEmpty(outcome.Errors!);
		Assert.Empty(await _db.StudentDocuments.ToListAsync());
		Assert.Empty(_app.Storage.PresignCalls);
	}

	[Fact]
	public async Task Refuses_when_too_many_unconfirmed_rows_are_already_pending()
	{
		for (var i = 0; i < PresignedUploadService.MaxPendingPerHour / 4; i++)
		{
			var ok = await _service.CreateAsync(
				Request(
					("registration", "image/jpeg", 1),
					("birth", "image/jpeg", 1),
					("baptism", "image/jpeg", 1),
					("address", "image/jpeg", 1)
				)
			);
			Assert.Equal(PresignStatus.Ok, ok.Status);
		}

		var blocked = await _service.CreateAsync(Request(("birth", "image/jpeg", 1)));

		Assert.Equal(PresignStatus.TooManyPending, blocked.Status);
		Assert.Equal(
			PresignedUploadService.MaxPendingPerHour,
			await _db.StudentDocuments.CountAsync()
		);
	}

	[Fact]
	public async Task Confirmed_uploads_do_not_count_against_the_pending_cap()
	{
		for (var i = 0; i < PresignedUploadService.MaxPendingPerHour / 4; i++)
		{
			await _service.CreateAsync(
				Request(
					("registration", "image/jpeg", 1),
					("birth", "image/jpeg", 1),
					("baptism", "image/jpeg", 1),
					("address", "image/jpeg", 1)
				)
			);
		}
		(await _db.StudentDocuments.ToListAsync()).ForEach(r => r.MarkUploaded(1, DateTime.UtcNow));
		await _db.SaveChangesAsync();

		var next = await _service.CreateAsync(Request(("birth", "image/jpeg", 1)));

		Assert.Equal(PresignStatus.Ok, next.Status);
	}
}
