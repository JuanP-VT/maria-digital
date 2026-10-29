# Guadalupe Digital

Two independent projects: `frontend/` (React 19, Vite, TypeScript, npm) and `backend/` (.NET 10, ASP.NET Core controllers, EF Core + PostgreSQL).

This is a volunteer-run tool for a Spanish-speaking parish (Parroquia Nuestra Señora de Guadalupe, Sinaloa). User-facing text is in Spanish.

## Principles

- Remove friction: speed of use beats everything (keyboard-first admin entry, mobile-first parent flows).
- Near-zero running cost: free/low-tier hosting. Do not add paid services or heavy dependencies.
- Simplicity over complexity: plain EF Core queries, small API contracts, local components. No new abstractions or libraries without a clear need.
- Gradual digitalization: paper forms, OCR ingestion and online registration coexist. Do not break one to improve another.

## Architecture

- Backend layers in `backend/GuadalupeDigital/`: `Domain` (entities), `Application` (interfaces such as `IObjectStorage` and `ILLMService`, DTOs, services), `Infrastructure` (R2 storage, OpenAI, EF Core `AppDbContext`), `Controllers`. Implementations depend on `Application`, never the reverse.
- Object storage is Cloudflare R2 (the README still says undecided).
- Auth is a cookie session for admins (`__Host-Guadalupe.Admin`, HttpOnly, Secure). Mutating `/api` requests require an antiforgery token (checked in `Program.cs` before authentication). A fallback authorization policy makes every endpoint admin-only unless explicitly opened. Login is rate limited.
- In production the frontend host proxies `/api/*` to the backend so cookies stay same-origin; in dev the Vite proxy does this. Never point the browser at a separate API domain.
- Data Protection application name must stay `GuadalupeDigital` (see README, "Admin Authentication Deployment").

## Where to look

- `frontend/CLAUDE.md` and `backend/CLAUDE.md`: folder-specific rules, loaded when you work there.
- `frontend/UI_GUIDELINES.md`: the design system. Read before any UI work.
- `README.md`: product overview and deployment notes. Parts are outdated (layout, `pnpm`); trust the code and this file.

## Commands

Frontend (run from `frontend/`):

- `npm test` runs the unit and component tests once (Vitest). `npm run test:watch` for watch mode, `npm run test:coverage` for coverage.
- `npm run lint`
- `npm run build` (also type-checks with `tsc -b`)

Backend (run from `backend/`):

- `dotnet test GuadalupeDigital.slnx`
- If the API is running (Visual Studio or `dotnet run`) it locks the Debug build output; run `dotnet test GuadalupeDigital.slnx -c Release` instead.
- `dotnet csharpier check GuadalupeDigital.Tests` (formatting; use `format` to fix)

CI (`.github/workflows/ci.yml`) runs all of the above on every PR and push to `main`.

## Testing rules

- Every behavior change or bug fix comes with a test. For a bug, write the failing test first.
- Run the relevant suite before saying work is done, and report failures as they are. Never delete or weaken a test to make it pass.
- Tests never touch the real network, the real database, OpenAI or R2. Mock external services (`ILLMService`, `IObjectStorage`, `fetch`).

Frontend:

- Test files sit next to the code as `*.test.ts(x)`. Shared setup is `src/test/setup.ts` (fake IndexedDB, object URL stubs).
- Prefer testing behavior through rendered output and hooks (`renderHook`) over internals.
- Do not write tests for `src/components/ui/**` (generated shadcn/Base UI components).
- Browser APIs missing from jsdom (camera, wake lock) are stubbed inside the test that needs them.

Backend:

- Tests live in `backend/GuadalupeDigital.Tests`, mirroring the app's folders.
- Integration tests use `TestApp` (`Infrastructure/TestApp.cs`): the real app on in-memory SQLite, with a seeded admin. Create one per test (`new TestApp()`, dispose it) so rate-limit state and data never leak between tests. Use `CreateBrowser()` for a cookie-keeping HTTPS client.
- Mutating `/api` requests need a CSRF token: `await client.GetCsrfTokenAsync()` and send it as `X-CSRF-TOKEN` (see `PostJsonAsync`). Fetch it before logging in and reuse it, because the CSRF check runs before authentication.
- SQLite is not PostgreSQL. If a feature depends on Postgres-specific behavior, say so and discuss switching to Testcontainers (needs Docker) rather than working around it.
- New endpoints are admin-only by default; `EndpointSecurityTests` fails if an endpoint is unintentionally public.
