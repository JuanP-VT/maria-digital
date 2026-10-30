using System.Net;
using MariaDigital.Tests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MariaDigital.Tests.Controllers;

public sealed class EndpointSecurityTests : IDisposable
{
	private readonly TestApp _app = new();

	public void Dispose() => _app.Dispose();

	// The GET check below cannot see a public POST, so the full list of anonymous endpoints is pinned
	// here. Opening a new one must be a deliberate edit of this list.
	[Fact]
	public void Only_the_intended_endpoints_are_anonymous()
	{
		var anonymous = _app
			.Services.GetServices<EndpointDataSource>()
			.SelectMany(source => source.Endpoints)
			.OfType<RouteEndpoint>()
			.Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
			.SelectMany(endpoint =>
				endpoint
					.Metadata.GetMetadata<HttpMethodMetadata>()!
					.HttpMethods.Select(method =>
						$"{method} /{endpoint.RoutePattern.RawText!.TrimStart('/')}"
					)
			)
			.Order()
			.ToList();

		Assert.Equal(
			[
				"GET /api/auth/csrf",
				"POST /api/auth/login",
				// Signed by the Cloudflare Worker (HMAC); no browser session or CSRF token.
				"POST /api/internal/uploads/confirmed",
			],
			anonymous
		);
	}

	// Every endpoint is admin-only unless it opts out with [AllowAnonymous]. This catches a new
	// endpoint that is accidentally public, or an opt-out that should not be there.
	[Fact]
	public async Task Every_parameterless_get_endpoint_without_AllowAnonymous_rejects_anonymous_users()
	{
		var endpoints = _app
			.Services.GetServices<EndpointDataSource>()
			.SelectMany(source => source.Endpoints)
			.OfType<RouteEndpoint>()
			.Where(endpoint =>
				endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null
				&& endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains("GET")
					== true
				&& !endpoint.RoutePattern.Parameters.Any()
			)
			.ToList();
		Assert.NotEmpty(endpoints);

		using var client = _app.CreateBrowser();
		foreach (var endpoint in endpoints)
		{
			var path = "/" + endpoint.RoutePattern.RawText!.TrimStart('/');
			var response = await client.GetAsync(path);
			Assert.True(
				response.StatusCode == HttpStatusCode.Unauthorized,
				$"GET {path} returned {(int)response.StatusCode} to an anonymous user"
			);
		}
	}
}
