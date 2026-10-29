using System.Net;
using System.Net.Http.Json;
using MariaDigital.Controllers;
using MariaDigital.Tests.Infrastructure;

namespace MariaDigital.Tests.Controllers;

public sealed class AuthControllerTests : IDisposable
{
	private readonly TestApp _app = new();
	private readonly HttpClient _client;

	public AuthControllerTests() => _client = _app.CreateBrowser();

	public void Dispose()
	{
		_client.Dispose();
		_app.Dispose();
	}

	[Fact]
	public async Task Csrf_returns_a_token_to_anonymous_users()
	{
		var token = await _client.GetCsrfTokenAsync();

		Assert.False(string.IsNullOrWhiteSpace(token));
	}

	[Fact]
	public async Task Login_with_valid_credentials_returns_the_admin_and_starts_a_session()
	{
		var response = await _client.LoginAsync();

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var session = await response.Content.ReadFromJsonAsync<AdminSessionResponse>();
		Assert.Equal(new AdminSessionResponse(TestApp.AdminName, TestApp.AdminEmail), session);

		var me = await _client.GetAsync("/api/auth/me");
		Assert.Equal(HttpStatusCode.OK, me.StatusCode);
	}

	[Fact]
	public async Task Login_ignores_email_case_and_surrounding_spaces()
	{
		var response = await _client.LoginAsync(email: "  ADMIN@Parroquia.MX ");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task Login_with_wrong_password_is_unauthorized()
	{
		var response = await _client.LoginAsync(password: "wrong");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.Equal(
			HttpStatusCode.Unauthorized,
			(await _client.GetAsync("/api/auth/me")).StatusCode
		);
	}

	[Fact]
	public async Task Login_with_unknown_email_is_unauthorized()
	{
		var response = await _client.LoginAsync(email: "nobody@parroquia.mx");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task Login_without_a_csrf_token_is_rejected()
	{
		await _client.GetCsrfTokenAsync(); // gets the cookie, but we omit the header
		var response = await _client.PostJsonAsync(
			"/api/auth/login",
			new LoginRequest(TestApp.AdminEmail, TestApp.AdminPassword),
			csrfToken: null
		);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task Me_requires_a_session()
	{
		var response = await _client.GetAsync("/api/auth/me");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task Logout_requires_a_session()
	{
		var token = await _client.GetCsrfTokenAsync();
		var response = await _client.PostJsonAsync("/api/auth/logout", new { }, token);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task Logout_ends_the_session()
	{
		// Reuse the pre-login token like the frontend does: the CSRF check runs before authentication,
		// so it validates tokens as an anonymous user (a token fetched after login is rejected).
		var token = await _client.GetCsrfTokenAsync();
		await _client.PostJsonAsync(
			"/api/auth/login",
			new LoginRequest(TestApp.AdminEmail, TestApp.AdminPassword),
			token
		);

		var response = await _client.PostJsonAsync("/api/auth/logout", new { }, token);

		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
		Assert.Equal(
			HttpStatusCode.Unauthorized,
			(await _client.GetAsync("/api/auth/me")).StatusCode
		);
	}

	[Fact]
	public async Task Login_is_rate_limited_after_five_attempts()
	{
		var token = await _client.GetCsrfTokenAsync();
		var attempt = new LoginRequest(TestApp.AdminEmail, "wrong");

		for (var i = 0; i < 5; i++)
		{
			var response = await _client.PostJsonAsync("/api/auth/login", attempt, token);
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		var limited = await _client.PostJsonAsync("/api/auth/login", attempt, token);
		Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
	}
}
