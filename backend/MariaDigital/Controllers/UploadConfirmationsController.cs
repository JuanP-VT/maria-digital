using System.Text;
using System.Text.Json;
using MariaDigital.Application.DTOs.Requests;
using MariaDigital.Application.Services;
using MariaDigital.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace MariaDigital.Controllers;

// Called by the Cloudflare Worker, not by a browser: it has no admin cookie or CSRF token, so it is
// opened to anonymous callers and authenticated by an HMAC signature instead (see Program.cs for
// the antiforgery exemption, which is limited to this exact path).
[ApiController]
[AllowAnonymous]
public sealed class UploadConfirmationsController(
	UploadConfirmationService service,
	IOptions<UploadWebhookOptions> options
) : ControllerBase
{
	public const string ConfirmedPath = "/api/internal/uploads/confirmed";
	public const string TimestampHeader = "X-Timestamp";
	public const string SignatureHeader = "X-Signature";

	[HttpPost(ConfirmedPath)]
	[EnableRateLimiting("webhook")]
	[RequestSizeLimit(4096)]
	public async Task<IActionResult> Confirmed(CancellationToken cancellationToken)
	{
		var settings = options.Value;
		// Fail closed: without a secret nobody can sign, so nothing is accepted.
		if (!UploadWebhookOptions.IsComplete(settings))
			return StatusCode(StatusCodes.Status503ServiceUnavailable);

		using var reader = new StreamReader(Request.Body, Encoding.UTF8);
		var body = await reader.ReadToEndAsync(cancellationToken);
		if (
			!WebhookSignature.IsValid(
				settings.Secret,
				Request.Headers[TimestampHeader].ToString(),
				Request.Headers[SignatureHeader].ToString(),
				body,
				DateTimeOffset.UtcNow
			)
		)
			return Unauthorized();

		UploadConfirmedRequest? request;
		try
		{
			request = JsonSerializer.Deserialize<UploadConfirmedRequest>(
				body,
				JsonSerializerOptions.Web
			);
		}
		catch (JsonException)
		{
			return BadRequest(new { message = "Cuerpo inválido." });
		}

		return await service.ConfirmAsync(request, cancellationToken) switch
		{
			UploadConfirmationStatus.Confirmed => NoContent(),
			UploadConfirmationStatus.UnknownKey => NotFound(
				new { message = "Documento no encontrado." }
			),
			_ => BadRequest(new { message = "Cuerpo inválido." }),
		};
	}
}
