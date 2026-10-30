# MariaDigital ⛪

**MariaDigital** is a web-based administration platform designed to digitize, streamline, and manage student enrollments, document archives, and diocesan metrics for the **Parroquia Nuestra Señora de Guadalupe** in Juan José Ríos, Sinaloa.

It replaces paper-based enrollment forms with a centralized digital archive, structured document storage (Cloudflare R2), and automated statistical reporting for local diocese requirements.

---

## 🚀 Key Features

- **Digital Document Archival:** Secure storage and association of student documents (Baptism Certificates, Birth Certificates, Enrollment Forms) in Cloudflare R2.
- **Document Capture Wizard:** Mobile-first, camera-based capture of each document with quality checks (blur/dark), preview and retake.
- **Diocese Statistical Dashboard:** Instant metrics reporting grouped by catechesis stage, age, family status, and auxiliary ministries (Choir, Altar Servers).
- **Fast Administration Portal:** Keyboard-optimized grid view for quick data entry and verification by administrators.
- **Online Public Enrollment (Phase 2):** Mobile-first public portal allowing parents to register students and upload documentation via QR/link.

---

## 🛠 Tech Stack

### **Backend**
- **Framework:** .NET 10 (C#) ASP.NET Core Web API (controllers)
- **Database:** PostgreSQL via Entity Framework Core
- **Object Storage:** Cloudflare R2 (behind the `IObjectStorage` abstraction)
- **Document extraction:** OpenAI (behind the `ILLMService` abstraction)
- **Tests:** xUnit, NSubstitute, in-memory SQLite via `WebApplicationFactory`

### **Frontend**
- **Framework:** React 19 (Vite + TypeScript), npm
- **UI & Styling:** Tailwind CSS 4 / shadcn/ui (Base UI), see [`frontend/UI_GUIDELINES.md`](frontend/UI_GUIDELINES.md)
- **Tests:** Vitest, Testing Library, jsdom

---

## 🗄 Core Domain Entities

Planned model (only `Student`, `RegistrationForm` and `Admin` exist so far):

```text
[ Capilla / Center ] ──< [ Group ] ──< [ Student ] ──< [ Document ]
                                            │
                                            ├──< [ Sacrament ]
                                            └──< [ Tutor ]
```

---

## 🎯 Core Engineering Principles

- **Remove Friction:** Prioritize UX speed above all else. Whether it's high-speed keyboard navigation (`Tab` + `Enter`) for manual entry, batch processing for physical forms, or mobile-first QR enrollment for parents, the system must save time—never add administrative overhead.
- **Near-Zero Running Cost:** Designed to run comfortably within free/low-cost tiers (PostgreSQL on Supabase/Neon, Cloudflare R2 for zero-egress blob storage, Vercel/Render hosting). Operational costs should remain virtually $0 for the parish.
- **Simplicity Over Complexity:** Resist over-engineering. Favor straight-forward EF Core queries, lightweight API contracts, and clear UI components over complex abstractions or unnecessary dependencies.
- **Gradual Digitalization:** Accommodate real-world constraints. The system seamlessly supports a hybrid environment where paper forms, OCR ingestion, and direct online registration coexist without breaking workflows.

Design decisions and their reasons are recorded in [`docs/decisions.md`](docs/decisions.md).

---

## 📁 Project Structure

```text
maria-digital/
├── backend/
│   ├── MariaDigital/
│   │   ├── Controllers/          # HTTP endpoints
│   │   ├── Application/          # Interfaces, DTOs, services
│   │   ├── Domain/               # Entities
│   │   ├── Infrastructure/       # EF Core DbContext, R2 storage, OpenAI client, options
│   │   ├── Migrations/           # EF Core migrations (generated)
│   │   └── Program.cs            # Composition root, auth, CSRF, rate limiting
│   └── MariaDigital.Tests/   # xUnit tests (integration tests via TestApp)
├── frontend/
│   └── src/
│       ├── components/           # Feature folders (doc-wizard/) and generated ui/
│       ├── lib/                  # Shared helpers and hooks
│       └── test/                 # Vitest setup
├── worker/                       # Cloudflare Worker: R2 upload events (Queue) -> backend webhook
├── docs/                         # Decision log
└── .github/workflows/ci.yml      # Lint, test, build on every PR and push to main
```

AI-agent guidance lives in [`CLAUDE.md`](CLAUDE.md) (plus `backend/CLAUDE.md`, `frontend/CLAUDE.md` and `.claude/skills/`).

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) with npm
- A PostgreSQL instance (local, or free tier via [Supabase](https://supabase.com) / [Neon](https://neon.tech))
- Cloudflare R2 credentials and an OpenAI API key (for the ingestion features)

### Installation

```bash
git clone https://github.com/JuanP-VT/maria-digital.git
cd maria-digital

# Backend (https://localhost:7028)
cd backend
dotnet tool restore
dotnet restore
dotnet ef database update --project MariaDigital
dotnet run --project MariaDigital --launch-profile https

# Frontend (in a new terminal, http://localhost:5173)
cd frontend
npm ci
npm run dev
```

The Vite dev server proxies `/api` to the backend (override with `VITE_API_PROXY_TARGET`).

### Configuration

Keep secrets out of `appsettings.json`. Use `dotnet user-secrets`, environment variables, or the gitignored `appsettings.Development.json`:

```bash
ConnectionStrings__DefaultConnection="Host=localhost;Database=digital_guadalupe;Username=postgres;Password=postgres"
R2__AccountId="<account-id>"
R2__AccessKeyId="<access-key-id>"
R2__SecretAccessKey="<secret>"
R2__BucketName="guadalupe-archive"
UploadWebhook__Secret="<at least 32 random characters, same value as the Worker's secret>"
OpenAi__ApiKey="<key>"
```

> ⚠️ Never commit real credentials.

### Running the tests

```bash
cd frontend && npm test
cd backend && dotnet test MariaDigital.slnx
cd worker && npm test
```

### Upload Confirmation Worker Deployment

Browsers upload straight to R2 with presigned URLs, which leaves the `StudentDocument` row `Pending`. R2 reports every created object to a Cloudflare Queue, and the Worker in `worker/` reads that queue and calls `POST /api/internal/uploads/confirmed` on the backend, which flips the row to `Uploaded`. Delivery is at-least-once, so the endpoint is idempotent. The Worker retries with exponential backoff (a sleeping free-tier backend gets time to wake) and, after 8 attempts, moves the message to a dead-letter queue instead of losing it.

One-time setup (from `worker/`, with `npx wrangler login` done):

```bash
npm ci
npx wrangler queues create guadalupe-uploads-dlq
npx wrangler queues create guadalupe-uploads
npx wrangler r2 bucket notification create guadalupe-archive --event-type object-create --queue guadalupe-uploads --prefix submissions/
npx wrangler secret put UPLOAD_WEBHOOK_SECRET   # same value as the backend's UploadWebhook__Secret
# set API_BASE_URL in wrangler.toml, then:
npx wrangler deploy
```

`API_BASE_URL` can be the frontend origin: the host already proxies `/api/*` to the backend. Requests are signed with HMAC-SHA256 over `{timestamp}.{body}` and rejected after 5 minutes, and the endpoint is closed (503) until `UploadWebhook__Secret` is set. To replay messages that reached the dead-letter queue (for example after a wrong secret), attach a consumer to `guadalupe-uploads-dlq` temporarily or move them back with the Cloudflare dashboard.

### Admin Authentication Deployment

The admin dashboard uses an `HttpOnly`, secure session cookie. The frontend and API may run on different providers or in different regions, but production should expose the API under the frontend's origin at `/api/*` using the frontend host's rewrite/proxy feature. The browser then makes same-origin requests while the proxy forwards them to the backend over HTTPS. Do not point browser requests directly at an unrelated API domain; modern browsers may block its cross-site cookies.

Configure the backend's `ConnectionStrings__DefaultConnection` and provision the first admin once with `AdminBootstrap__Email`, `AdminBootstrap__Name`, and `AdminBootstrap__Password` environment variables. The password must be at least 12 characters. Bootstrap creates an account only while the admin table is empty; remove those bootstrap variables after the first successful startup. Configure `Cors__AllowedOrigins__0` only when intentionally serving the frontend from a separate origin, and use HTTPS for both services.

If the backend runs more than one instance, configure ASP.NET Core Data Protection to persist its key ring in shared durable storage accessible to every instance. Keep the application name set to `GuadalupeDigital` (or configure the same `DataProtection__ApplicationName` everywhere). Without shared keys, sessions created by one instance will not be readable by another and will be lost on restarts.

---

## 🗺️ Roadmap

- [ ] **Epic 1: Foundation & Core Infrastructure** — DB schema, EF Core migrations, R2 storage client, API/UI scaffolds
- [ ] **Epic 2: Ingestion Pipeline** — Image uploads, OCR Vision processing, split-screen validation UI, DB ingestion
- [ ] **Epic 3: Admin Dashboard & Diocesan Reporting** — Student directory, document viewer, automatic stats generation, Excel export
- [ ] **Epic 4: Public Online Enrollment** — Mobile-first registration wizard, public document uploads, admin approval queue

---

## 🤝 Contributing

This project is currently developed and maintained as an independent volunteer effort for the parish. If you'd like to contribute (dev, design, or parish-domain expertise), open an issue or reach out directly.

---

## 📄 License

MIT — free to use and adapt for other parishes or dioceses facing the same paper-archive problem.
