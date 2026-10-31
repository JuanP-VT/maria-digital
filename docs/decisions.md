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
- **Consequence:** Rows stay `Pending` until the Cloudflare Worker confirms the upload (see "Upload confirmation"). The bucket must stay private and its CORS must allow `PUT` only from the app origin. Unconfirmed rows are capped per hour to bound abuse.

## Wizard uploads on save, draft kept until success
- **Decision:** "Guardar estudiante" uploads the captured photos with `uploadDocuments` (presign, then parallel PUTs to R2) and only clears the IndexedDB draft after every file succeeded. The preview screen shows progress and offers "Reintentar" on failure.
- **Why:** Volunteers work on phone data in the parish hall. A failed upload must never cost them the photos, and parallel PUTs keep a four-photo save short.
- **Consequence:** A retry presigns again and creates new `Pending` rows for the same photos; the hourly cap on unconfirmed rows bounds this and the confirm Worker marks whichever objects actually arrive. The R2 bucket CORS must allow `PUT` from the app origin, otherwise the browser reports "Sin conexión" for every upload.

## Upload confirmation: R2 event, Queue, Worker, signed webhook
- **Decision:** R2 sends object-create events (prefix `submissions/`) to a Cloudflare Queue. A Worker (`worker/`) consumes it and calls `POST /api/internal/uploads/confirmed` with `{ key, size }`, signed with HMAC-SHA256 over `{unix timestamp}.{body}` (`X-Timestamp`, `X-Signature`, shared secret `UploadWebhook__Secret`). The backend rejects timestamps more than 5 minutes off, compares in constant time, and answers 503 while no secret is configured.
- **Why:** The browser cannot be trusted to say an upload happened, and polling R2 from the backend would need a scheduler. Queues give at-least-once delivery, retries and a dead-letter queue within the free tier. The Worker keeps no state: the backend dedupes by `StorageKey` (unique) and `MarkUploaded` is a no-op on repeat.
- **Retries:** Only a 2xx settles a message; everything else (including 401/403/404) is retried with exponential backoff and jitter (5s up to 10 min), 8 attempts, then the dead-letter queue. A 30s request timeout covers a sleeping free-tier backend. Unreadable messages and non-create events are acknowledged.
- **Consequence:** This is the one deliberate exception to "mutating `/api` requests need a CSRF token" and to "endpoints are admin-only": the exact path `/api/internal/uploads/confirmed` skips the antiforgery check and is `[AllowAnonymous]`, and the signature replaces both. `EndpointSecurityTests` pins the list of anonymous endpoints so any other opening is a deliberate edit. Rotating the secret means changing it on the Worker and the backend together; messages that failed meanwhile wait in the dead-letter queue.

## Dev queue poller replaces the Worker consumer temporarily
- **Decision:** `backend/MariaDigital.QueuePoller` is a dev-only console tool that pulls `guadalupe-uploads` over the Cloudflare Queues HTTP API and sends the same signed webhook the Worker sends, using the backend's own `WebhookSignature.Sign`. Parsing and backoff are ports of `worker/src/message.ts` and `retry.ts`.
- **Why:** Running the Worker against real buckets is slow to iterate on. The poller exercises the same backend path from a local debugger. It needs no R2 access: the Worker only uses `key` and `size` from the event.
- **Consequence:** A queue allows one consumer type, so the HTTP pull consumer replaces the Worker's push consumer while it is in use, and production confirmations pause until the Worker is re-attached (steps in the README). Keep the ports in sync if the Worker's message handling changes.

## Extraction runs locally, pulled by a console job
- **Decision:** Scanning of Done candidates runs on an admin's own machine with a local vision model served by Ollama (`backend/MariaDigital.LocalScanner`), instead of sending sheets to OpenAI. The job is started on demand and pulls work from the API; the cloud never calls the PC.
- **Why:** The sheets hold minors' personal data, and running costs must stay near zero. Pull-based means no open ports or tunnels into the volunteer's machine.
- **Consequence:** Ollama exposes an OpenAI-compatible API, so the existing `ILLMService` shape carries over. The first step only bootstraps Ollama and chats with the model; claiming jobs and posting results through the API come next. Model choice is settled by testing real sheets.
