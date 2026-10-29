using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MariaDigital.Domain.Admin;
using MariaDigital.Infrastructure.Persistance;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace MariaDigital.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
	AppDbContext db,
	IPasswordHasher<Admin> passwordHasher,
	IAntiforgery antiforgery
) : ControllerBase
{
	[HttpGet("csrf")]
	[AllowAnonymous]
	public IActionResult GetCsrfToken()
	{
		var tokens = antiforgery.GetAndStoreTokens(HttpContext);
		return Ok(new CsrfTokenResponse(tokens.RequestToken!));
	}

	[HttpPost("login")]
	[AllowAnonymous]
	[EnableRateLimiting("login")]
	public async Task<IActionResult> Login(LoginRequest request)
	{
		var email = request.Email.Trim().ToLowerInvariant();
		var admin = await db.Admins.SingleOrDefaultAsync(candidate => candidate.Email == email);
		if (
			admin is null
			|| passwordHasher.VerifyHashedPassword(admin, admin.HashedPassword, request.Password)
				== PasswordVerificationResult.Failed
		)
		{
			return Unauthorized(new { message = "Correo o contraseña incorrectos." });
		}

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
			new Claim(ClaimTypes.Name, admin.Username),
			new Claim(ClaimTypes.Email, admin.Email),
			new Claim(ClaimTypes.Role, "Admin"),
		};
		var identity = new ClaimsIdentity(
			claims,
			CookieAuthenticationDefaults.AuthenticationScheme
		);
		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(identity)
		);

		return Ok(new AdminSessionResponse(admin.Username, admin.Email));
	}

	[HttpGet("me")]
	[Authorize]
	public IActionResult GetCurrentAdmin()
	{
		return Ok(
			new AdminSessionResponse(
				User.Identity?.Name ?? string.Empty,
				User.FindFirstValue(ClaimTypes.Email) ?? string.Empty
			)
		);
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		return NoContent();
	}
}

public sealed record LoginRequest(
	[Required, EmailAddress] string Email,
	[Required] string Password
);

public sealed record CsrfTokenResponse(string RequestToken);

public sealed record AdminSessionResponse(string DisplayName, string Email);
