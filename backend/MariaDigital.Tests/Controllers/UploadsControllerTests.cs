using System.Net;
using System.Net.Http.Json;
using MariaDigital.Application.DTOs.Requests;
using MariaDigital.Application.DTOs.Responses;
using MariaDigital.Infrastructure.Persistance;
using MariaDigital.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MariaDigital.Tests.Controllers;

public sealed class UploadsControllerTests : IDisposable
{
	private const string Url = "/api/uploads/presign";

	private readonly TestApp _app = new();
	private readonly HttpClient _client;

	public UploadsControllerTests() => _client = _app.CreateBrowser();

	public void Dispose()
	{
		_client.Dispose();
		_app.Dispose();
	}

	private static PresignUploadsRequest ValidRequest() =>
		new([new PresignFileRequest("registration", "image/jpeg", 5000)]);

	private async Task<int> RowCountAsync()
	{
		using var scope = _app.Services.CreateScope();
		return await scope
			.ServiceProvider.GetRequiredService<AppDbContext>()
			.StudentDocuments.CountAsync();
	}

	[Fact]
	public async Task Anonymous_users_are_rejected_and_nothing_is_created()
	{
		var csrf = await _client.GetCsrfTokenAsync();

		var response = await _client.PostJsonAsync(Url, ValidRequest(), csrf);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.Equal(0, await RowCountAsync());
		Assert.Empty(_app.Storage.PresignCalls);
	}

	[Fact]
	public async Task Requests_without_a_csrf_token_are_rejected_even_for_a_logged_in_admin()
	{
		await _client.LoginAsync();

		var response = await _client.PostJsonAsync(Url, ValidRequest(), csrfToken: null);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.Equal(0, await RowCountAsync());
	}

	[Fact]
	public async Task A_csrf_token_fetched_while_logged_in_is_accepted()
	{
		await _client.LoginAsync();
		var csrf = await _client.GetCsrfTokenAsync();

		var response = await _client.PostJsonAsync(Url, ValidRequest(), csrf);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task Admin_gets_presigned_urls_and_pending_rows_are_stored()
	{
		var csrf = await _client.GetCsrfTokenAsync();
		await _client.LoginAsync();

		var response = await _client.PostJsonAsync(Url, ValidRequest(), csrf);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
		var body = await response.Content.ReadFromJsonAsync<PresignUploadsResponse>();
		var upload = Assert.Single(body!.Uploads);
		Assert.Equal("registration", upload.Type);
		Assert.Equal(1, await RowCountAsync());
	}

	[Fact]
	public async Task Invalid_input_returns_400_with_messages_and_creates_nothing()
	{
		var csrf = await _client.GetCsrfTokenAsync();
		await _client.LoginAsync();
		var bad = new PresignUploadsRequest([new PresignFileRequest("birth", "text/html", 5000)]);

		var response = await _client.PostJsonAsync(Url, bad, csrf);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.Equal(0, await RowCountAsync());
	}

	[Fact]
	public async Task The_endpoint_is_rate_limited()
	{
		var csrf = await _client.GetCsrfTokenAsync();
		await _client.LoginAsync();

		HttpStatusCode last = default;
		for (var i = 0; i < 11; i++)
			last = (await _client.PostJsonAsync(Url, ValidRequest(), csrf)).StatusCode;

		Assert.Equal(HttpStatusCode.TooManyRequests, last);
	}
}
