using System.Net.Http.Json;
using MariaDigital.Application;
using MariaDigital.Controllers;
using MariaDigital.Domain.Admin;
using MariaDigital.Infrastructure.Persistance;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MariaDigital.Tests.Infrastructure;

/// <summary>
/// The real app wired to an in-memory SQLite database instead of PostgreSQL.
/// Create one per test: it owns its own database and its own rate-limiter state.
/// </summary>
public sealed class TestApp : WebApplicationFactory<Program>
{
	public const string AdminEmail = "admin@parroquia.mx";
	public const string AdminName = "Padre Juan";
	public const string AdminPassword = "correct-horse-battery-staple";
	public const string WebhookSecret = "test-webhook-secret-0123456789-abcdef";

	private readonly SqliteConnection _connection = new("DataSource=:memory:");

	/// <summary>Stands in for R2; inspect <see cref="FakeObjectStorage.PresignCalls"/>.</summary>
	public FakeObjectStorage Storage { get; } = new();

	public TestApp()
	{
		_connection.Open();
		using var scope = Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		db.Database.EnsureCreated();

		var admin = new Admin(AdminName, string.Empty, AdminEmail);
		var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Admin>>();
		admin.SetHashedPassword(hasher.HashPassword(admin, AdminPassword));
		db.Admins.Add(admin);
		db.SaveChanges();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration(
			(_, config) =>
				// A developer's appsettings.Local.json must never make tests bootstrap a real admin.
				config.AddInMemoryCollection(
					new Dictionary<string, string?>
					{
						["AdminBootstrap:Email"] = null,
						["AdminBootstrap:Name"] = null,
						["AdminBootstrap:Password"] = null,
						["UploadWebhook:Secret"] = WebhookSecret,
					}
				)
		);

		builder.ConfigureServices(services =>
		{
			services.RemoveAll<DbContextOptions<AppDbContext>>();
			services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
			services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

			services.RemoveAll<IObjectStorage>();
			services.AddSingleton<IObjectStorage>(Storage);
		});
	}

	/// <summary>Cookie-keeping HTTPS client (the auth and CSRF cookies are Secure and __Host- prefixed).</summary>
	public HttpClient CreateBrowser() =>
		CreateClient(
			new WebApplicationFactoryClientOptions
			{
				BaseAddress = new Uri("https://localhost"),
				HandleCookies = true,
				AllowAutoRedirect = false,
			}
		);

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
			_connection.Dispose();
	}
}

public static class HttpClientExtensions
{
	public static async Task<string> GetCsrfTokenAsync(this HttpClient client)
	{
		var response = await client.GetFromJsonAsync<CsrfTokenResponse>("/api/auth/csrf");
		return response!.RequestToken;
	}

	public static Task<HttpResponseMessage> PostJsonAsync<T>(
		this HttpClient client,
		string url,
		T body,
		string? csrfToken
	)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, url)
		{
			Content = JsonContent.Create(body),
		};
		if (csrfToken is not null)
			request.Headers.Add("X-CSRF-TOKEN", csrfToken);
		return client.SendAsync(request);
	}

	public static async Task<HttpResponseMessage> LoginAsync(
		this HttpClient client,
		string email = TestApp.AdminEmail,
		string password = TestApp.AdminPassword
	)
	{
		var token = await client.GetCsrfTokenAsync();
		return await client.PostJsonAsync(
			"/api/auth/login",
			new LoginRequest(email, password),
			token
		);
	}
}
