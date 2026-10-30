using MariaDigital.Application.DTOs.Requests;
using MariaDigital.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MariaDigital.Controllers;

// Admin-only (fallback policy) and antiforgery-protected (Program.cs); not opened to the public.
[ApiController]
[Route("api/uploads")]
public sealed class UploadsController(PresignedUploadService service) : ControllerBase
{
	[HttpPost("presign")]
	[EnableRateLimiting("presign")]
	public async Task<IActionResult> Presign(
		PresignUploadsRequest request,
		CancellationToken cancellationToken
	)
	{
		// The response carries live upload URLs.
		Response.Headers.CacheControl = "no-store";

		var outcome = await service.CreateAsync(request, cancellationToken);
		return outcome.Status switch
		{
			PresignStatus.Ok => Ok(outcome.Response),
			PresignStatus.TooManyPending => StatusCode(
				StatusCodes.Status429TooManyRequests,
				new { message = "Demasiadas cargas pendientes. Intenta más tarde." }
			),
			_ => BadRequest(new { errors = outcome.Errors }),
		};
	}
}
