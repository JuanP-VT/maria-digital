# Backend

.NET 10 ASP.NET Core controllers API in `MariaDigital/`, xUnit tests in `MariaDigital.Tests/`. Commands and testing rules are in the root `CLAUDE.md`.

## Conventions

- Format with CSharpier (`.csharpierrc.json`: tabs, width 100). All code is formatted except `Migrations/` (`.csharpierignore`). A hook in `.claude/settings.json` formats edited `.cs` files, and CI runs `dotnet csharpier check .`.
- Keep logic in small, pure, unit-testable methods; controllers and services stay thin (see the root `CLAUDE.md` testing rules).
- Nullable reference types are on; keep it warning-free.
- Layering: entities in `Domain`, interfaces and DTOs in `Application`, implementations in `Infrastructure`. Register services in `Program.cs`.
- New endpoint: controller in `Controllers/`, request/response DTOs in `Application/DTOs`, logic in an `Application/Services` class when it is more than a trivial query. Endpoints are admin-only by default; opening one to the public needs an explicit decision and an update to `EndpointSecurityTests`.
- Mutating endpoints sit behind antiforgery validation automatically; do not bypass it.

## Database

- EF Core with PostgreSQL (SQLite in tests). Add a migration with `dotnet ef migrations add <Name> --project MariaDigital` from `backend/`, then review the generated files.
- Never hand-edit `Migrations/*.Designer.cs` or `AppDbContextModelSnapshot.cs`.

## Secrets

- Connection strings, R2 keys and OpenAI keys come from environment variables or user-secrets, never `appsettings.json`. `appsettings.Development.json` and `appsettings.Local.json` are gitignored; do not read or print them.
