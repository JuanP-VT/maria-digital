using System.Net;
using System.Text;
using System.Text.Json;
using MariaDigital.Application.Services;
using MariaDigital.Controllers;
using MariaDigital.Domain.Students;
using MariaDigital.Infrastructure.Persistance;
using MariaDigital.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MariaDigital.Tests.Controllers;

public sealed class UploadConfirmationsControllerTests : IDisposable
{
	private readonly TestApp _app = new();
	private readonly HttpClient _client;

	public UploadConfirmationsControllerTests() => _client = _app.CreateBrowser();

	public void Dispose()
	{
		_client.Dispose();
		_app.Dispose();
	}

	private async Task<string> SeedPendingAsync()
	{
		using var scope = _app.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var row = StudentDocument.CreatePending(
			Guid.NewGuid(),
			StudentDocumentType.Registration,
			"image/jpeg"
		);
		db.StudentDocuments.Add(row);
		await db.SaveChangesAsync();
		return row.StorageKey;
	}

	private async Task<StudentDocument> LoadAsync(string key)
	{
		using var scope = _app.Services.CreateScope();
		return await scope
			.ServiceProvider.GetRequiredService<AppDbContext>()
			.StudentDocuments.AsNoTracking()
			.SingleAsync(d => d.StorageKey == key);
	}

	private static HttpRequestMessage SignedRequest(
		string body,
		string secret = TestApp.WebhookSecret,
		DateTimeOffset? at = null
	)
	{
		var timestamp = (at ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
		var request = new HttpRequestMessage(
			HttpMethod.Post,
			UploadConfirmationsController.ConfirmedPath
		)
		{
			Content = new StringContent(body, Encoding.UTF8, "application/json"),
		};
		request.Headers.Add(UploadConfirmationsController.TimestampHeader, timestamp.ToString());
		request.Headers.Add(
			UploadConfirmationsController.SignatureHeader,
			WebhookSignature.Sign(secret, timestamp, body)
		);
		return request;
	}

	private static string Body(string key, long size = 5000) =>
		JsonSerializer.Serialize(new { key, size });

	[Fact]
	public async Task A_signed_request_marks_the_document_uploaded_without_a_csrf_token()
	{
		var key = await SeedPendingAsync();

		var response = await _client.SendAsync(SignedRequest(Body(key, 1234)));

		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
		var row = await LoadAsync(key);
		Assert.Equal(DocumentStatus.Uploaded, row.Status);
		Assert.Equal(1234, row.SizeBytes);
		Assert.NotNull(row.UploadedAt);
	}

	[Fact]
	public async Task A_repeated_delivery_succeeds_and_keeps_the_first_confirmation()
	{
		var key = await SeedPendingAsync();
		await _client.SendAsync(SignedRequest(Body(key, 1234)));
		var first = await LoadAsync(key);

		var response = await _client.SendAsync(SignedRequest(Body(key, 9999)));

		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
		var second = await LoadAsync(key);
		Assert.Equal(first.UploadedAt, second.UploadedAt);
		Assert.Equal(1234, second.SizeBytes);
	}

	[Fact]
	public async Task A_wrong_signature_is_rejected_and_the_row_stays_pending()
	{
		var key = await SeedPendingAsync();

		var response = await _client.SendAsync(SignedRequest(Body(key), secret: "wrong-secret"));

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.Equal(DocumentStatus.Pending, (await LoadAsync(key)).Status);
	}

	[Fact]
	public async Task A_request_without_signature_headers_is_rejected()
	{
		var key = await SeedPendingAsync();
		var request = new HttpRequestMessage(
			HttpMethod.Post,
			UploadConfirmationsController.ConfirmedPath
		)
		{
			Content = new StringContent(Body(key), Encoding.UTF8, "application/json"),
		};

		var response = await _client.SendAsync(request);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task A_stale_timestamp_is_rejected()
	{
		var key = await SeedPendingAsync();

		var response = await _client.SendAsync(
			SignedRequest(Body(key), at: DateTimeOffset.UtcNow.AddMinutes(-10))
		);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.Equal(DocumentStatus.Pending, (await LoadAsync(key)).Status);
	}

	[Fact]
	public async Task An_unknown_key_returns_404_so_the_worker_can_retry()
	{
		var response = await _client.SendAsync(SignedRequest(Body("submissions/none/birth/none")));

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Theory]
	[InlineData("""{"key":"","size":5}""")]
	[InlineData("""{"size":5}""")]
	[InlineData("""{"key":"a","size":-1}""")]
	[InlineData("not json")]
	public async Task A_signed_but_malformed_body_returns_400(string body)
	{
		var response = await _client.SendAsync(SignedRequest(body));

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task Without_a_configured_secret_every_request_is_refused()
	{
		var key = await SeedPendingAsync();
		using var unconfigured = _app.WithWebHostBuilder(builder =>
			builder.ConfigureAppConfiguration(
				(_, config) =>
					config.AddInMemoryCollection(
						new Dictionary<string, string?> { ["UploadWebhook:Secret"] = "" }
					)
			)
		);
		using var client = unconfigured.CreateClient(
			new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
			{
				BaseAddress = new Uri("https://localhost"),
			}
		);

		var response = await client.SendAsync(SignedRequest(Body(key)));

		Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
		Assert.Equal(DocumentStatus.Pending, (await LoadAsync(key)).Status);
	}

	[Fact]
	public async Task Other_mutating_endpoints_still_require_a_csrf_token()
	{
		var response = await _client.PostJsonAsync("/api/auth/login", new { }, csrfToken: null);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}
}
