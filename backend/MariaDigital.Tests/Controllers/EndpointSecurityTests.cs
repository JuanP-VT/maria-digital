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
