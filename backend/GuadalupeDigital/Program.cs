using System.Security.Claims;
using System.Threading.RateLimiting;
using GuadalupeDigital.Domain.Admin;
using GuadalupeDigital.Infrastructure.Persistance;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
	var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
	policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.Cookie.Name = "__Host-Guadalupe.Admin";
		options.Cookie.Path = "/";
		options.Cookie.HttpOnly = true;
		options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
		options.Cookie.SameSite = SameSiteMode.Lax;
		options.ExpireTimeSpan = TimeSpan.FromHours(8);
		options.SlidingExpiration = true;
		options.Events.OnRedirectToLogin = context =>
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return Task.CompletedTask;
		};
		options.Events.OnRedirectToAccessDenied = context =>
		{
			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			return Task.CompletedTask;
		};
	});
builder.Services.AddDataProtection()
	.SetApplicationName(builder.Configuration["DataProtection:ApplicationName"] ?? "GuadalupeDigital");

builder.Services.AddAuthorization(options =>
{
	options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
		.RequireClaim(ClaimTypes.Role, "Admin")
		.Build();
});

builder.Services.AddAntiforgery(options =>
{
	options.HeaderName = "X-CSRF-TOKEN";
	options.Cookie.Name = "__Host-Guadalupe.Csrf";
	options.Cookie.Path = "/";
	options.Cookie.HttpOnly = true;
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
	options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddRateLimiter(options =>
{
	options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
		context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
		_ => new FixedWindowRateLimiterOptions
		{
			PermitLimit = 5,
			Window = TimeSpan.FromMinutes(1),
			QueueLimit = 0,
			AutoReplenishment = true,
		}));
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register PostgreSQL DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

var bootstrapEmail = app.Configuration["AdminBootstrap:Email"];
var bootstrapName = app.Configuration["AdminBootstrap:Name"];
var bootstrapPassword = app.Configuration["AdminBootstrap:Password"];
if (bootstrapEmail is not null || bootstrapName is not null || bootstrapPassword is not null)
{
	if (string.IsNullOrWhiteSpace(bootstrapEmail) || string.IsNullOrWhiteSpace(bootstrapName) ||
		string.IsNullOrWhiteSpace(bootstrapPassword))
	{
		throw new InvalidOperationException("Admin bootstrap requires an email, a name, and a password of at least 12 characters.");
	}

	await using var scope = app.Services.CreateAsyncScope();
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	if (!await db.Admins.AnyAsync())
	{
		var admin = new Admin(bootstrapName.Trim(), string.Empty, bootstrapEmail.Trim().ToLowerInvariant());
		var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Admin>>();
		admin.SetHashedPassword(hasher.HashPassword(admin, bootstrapPassword));
		db.Admins.Add(admin);
		await db.SaveChangesAsync();
	}
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseRateLimiter();
app.Use(async (context, next) =>
{
	var isMutation = HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) ||
		HttpMethods.IsPatch(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method);
	if (isMutation && context.Request.Path.StartsWithSegments("/api"))
	{
		try
		{
			await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
		}
		catch (AntiforgeryValidationException)
		{
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			await context.Response.WriteAsJsonAsync(new { message = "Invalid CSRF token." });
			return;
		}
	}

	await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
