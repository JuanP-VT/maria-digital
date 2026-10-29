# Backend

.NET 10 ASP.NET Core controllers API in `GuadalupeDigital/`, xUnit tests in `GuadalupeDigital.Tests/`. Commands and testing rules are in the root `CLAUDE.md`.

## Conventions

- Format with CSharpier (`.csharpierrc.json`: tabs, width 100). CI currently checks only the Tests project because older app code predates CSharpier; a hook in `.claude/settings.json` auto-formats edited files in the Tests project only. Do not reformat unrelated app code in a feature change.
- Nullable reference types are on; keep it warning-free.
- Layering: entities in `Domain`, interfaces and DTOs in `Application`, implementations in `Infrastructure`. Register services in `Program.cs`.
- New endpoint: controller in `Controllers/`, request/response DTOs in `Application/DTOs`, logic in an `Application/Services` class when it is more than a trivial query. Endpoints are admin-only by default; opening one to the public needs an explicit decision and an update to `EndpointSecurityTests`.
- Mutating endpoints sit behind antiforgery validation automatically; do not bypass it.

## Database

- EF Core with PostgreSQL (SQLite in tests). Add a migration with `dotnet ef migrations add <Name> --project GuadalupeDigital` from `backend/`, then review the generated files.
- Never hand-edit `Migrations/*.Designer.cs` or `AppDbContextModelSnapshot.cs`.

## Secrets

- Connection strings, R2 keys and OpenAI keys come from environment variables or user-secrets, never `appsettings.json`. `appsettings.Development.json` and `appsettings.Local.json` are gitignored; do not read or print them.
