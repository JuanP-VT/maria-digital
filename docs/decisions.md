# Decision log

Short records of choices that shaped the project, so they are not reopened by accident. Add a new entry when you make a decision that is expensive to reverse or that a newcomer would question. Newest last.

## Cloudflare R2 for document storage
- **Decision:** Store scanned documents in Cloudflare R2, behind `IObjectStorage`.
- **Why:** Zero egress fees keep running cost near $0 (principle: near-zero running cost). Clients upload with presigned PUT URLs, so files never pass through the API.
- **Consequence:** Azure Blob remains possible by writing another `IObjectStorage`; the README used to list this as undecided.

## Same-origin `/api` proxy, cookie auth
- **Decision:** Admins authenticate with an HttpOnly `__Host-` cookie. In production the frontend host proxies `/api/*` to the backend; in dev the Vite proxy does.
- **Why:** The frontend and API may live on different free-tier providers. Cross-site cookies are blocked by modern browsers, so the browser must see one origin.
- **Consequence:** Mutating requests need an antiforgery token (`GET /api/auth/csrf`). Never point the browser at a separate API domain.

## Endpoints are admin-only by default
- **Decision:** A fallback authorization policy requires an authenticated admin everywhere; public endpoints need an explicit `[AllowAnonymous]`.
- **Why:** The data is children's records. A forgotten attribute should fail closed.
- **Consequence:** `EndpointSecurityTests` fails when an endpoint becomes public unintentionally.

## LLM-based form ingestion
- **Decision:** Extract data from photographed registration forms with an LLM (`ILLMService`, OpenAI implementation) instead of classic OCR or manual entry only.
- **Why:** Handwritten Spanish forms vary widely; gradual digitalization means paper keeps arriving. An admin still verifies extracted data in a split-screen UI.
- **Consequence:** Extraction is behind an interface so the provider can change, and tests mock it.

## Capture quality checks run on the device
- **Decision:** The wizard flags blurry/dark photos client-side (`src/lib/image.ts`) and offers retake.
- **Why:** Parents and volunteers photograph documents on phones; catching bad photos immediately avoids a second trip. The backend remains the real judge.

## Integration tests on SQLite, not PostgreSQL
- **Decision:** Backend integration tests run the real app on in-memory SQLite (`TestApp`).
- **Why:** Fast, no Docker needed on CI or on a volunteer's laptop.
- **Consequence:** Postgres-specific behavior is not covered. If a feature needs it, discuss moving to Testcontainers instead of working around SQLite.

## Hash-based routing
- **Decision:** A tiny hook (`useHashRoute`) instead of a router library.
- **Why:** Few screens, no server-side rendering, static hosting works without rewrite rules (principle: simplicity over complexity). Revisit if routes need nesting, loaders or guards beyond the admin gate.

## Testability as a design constraint
- **Decision:** Logic is extracted into small pure functions; controllers, services and components stay thin. Most code gets a test.
- **Why:** The project is built largely with AI assistance, and tests are the safety net that lets generated code be accepted quickly.

## Presigned uploads: server-owned keys, signed type and size
- **Decision:** `POST /api/uploads/presign` (admin-only) validates the files, saves `Pending` `StudentDocuments` rows, then returns 5-minute presigned PUT URLs. The server generates every storage key; the client sends only type, content type and size. Content type and length are part of the signature.
- **Why:** The student does not exist yet at upload time (a later job reads the registration sheet), so files are grouped by a `SubmissionId`. A leaked or tampered URL can only write one object of the declared type and size, once, for a few minutes. The URL is created only after its row exists.
- **Consequence:** Rows stay `Pending` until a Cloudflare Worker confirms the upload (not built yet). The bucket must stay private and its CORS must allow `PUT` only from the app origin. Unconfirmed rows are capped per hour to bound abuse.

## Wizard uploads on save, draft kept until success
- **Decision:** "Guardar estudiante" uploads the captured photos with `uploadDocuments` (presign, then parallel PUTs to R2) and only clears the IndexedDB draft after every file succeeded. The preview screen shows progress and offers "Reintentar" on failure.
- **Why:** Volunteers work on phone data in the parish hall. A failed upload must never cost them the photos, and parallel PUTs keep a four-photo save short.
- **Consequence:** A retry presigns again and creates new `Pending` rows for the same photos; the hourly cap on unconfirmed rows bounds this and the future confirm Worker can reconcile. The R2 bucket CORS must allow `PUT` from the app origin, otherwise the browser reports "Sin conexión" for every upload.
