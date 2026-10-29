# Guadalupe Digital

Two independent projects: `frontend/` (React 19, Vite, TypeScript, npm) and `backend/` (.NET 10, ASP.NET Core controllers, EF Core + PostgreSQL).

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
