---
name: add-endpoint
description: Add a new backend API endpoint with its DTOs, service logic, and tests. Use when creating or extending a controller action.
---

# Add an API endpoint

1. **DTOs:** request and response records in `backend/MariaDigital/Application/DTOs/` (see `Requests/CreatePresignedPutRequest.cs`). Validate with data annotations.
2. **Logic:** put anything beyond a trivial query in an `Application/Services/` class, split into small pure methods (input in, result out) so it can be unit tested without HTTP or a database. Depend on `IObjectStorage` / `ILLMService`, never on the concrete R2 or OpenAI classes. Register new services in `Program.cs`.
3. **Controller:** thin action in `Controllers/` under `[Route("api/...")]` that calls the service and maps the result to a status code (see `AuthController`). Endpoints are admin-only by default; only add `[AllowAnonymous]` when the user explicitly wants it public, and then add a rate limit if it can be abused.
4. **Messages:** user-visible error text is in Spanish.
5. **Tests:**
   - Unit tests for the extracted logic.
   - An integration test with `TestApp` for the endpoint: success, validation failure, and unauthenticated (expect 401). Mutating requests need the CSRF token (`GetCsrfTokenAsync`, `X-CSRF-TOKEN`); see `AuthControllerTests`.
   - If the endpoint is intentionally public, update `EndpointSecurityTests`.
   - Mock `ILLMService` and `IObjectStorage`; never hit the network.
6. Format with `dotnet csharpier format .` and run `dotnet test MariaDigital.slnx -c Release` from `backend/`.
