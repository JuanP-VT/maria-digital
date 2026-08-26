# Guadalupe Digital ⛪
 
**Guadalupe Digital** is a web-based administration platform designed to digitize, streamline, and manage student enrollments, document archives, and diocesan metrics for the **Parroquia Nuestra Señora de Guadalupe** in Juan José Ríos, Sinaloa.
 
It replaces paper-based enrollment forms with a centralized digital archive, structured document storage (Blob Storage), and automated statistical reporting for local diocese requirements.
 
---
 
## 🚀 Key Features
 
- **Digital Document Archival:** Secure storage and association of student documents (Baptism Certificates, Birth Certificates, Enrollment Forms) using Cloud Blob Storage.
- **Diocese Statistical Dashboard:** Instant metrics reporting grouped by catechesis stage, age, family status, and auxiliary ministries (Choir, Altar Servers).
- **Fast Administration Portal:** Keyboard-optimized grid view for quick data entry and verification by administrators.
- **Online Public Enrollment (Phase 2):** Mobile-first public portal allowing parents to register students and upload documentation via QR/link.
---
 
## 🛠 Tech Stack
 
### **Backend**
- **Framework:** .NET 10 (C#) Web API
- **Database:** PostgreSQL (ORM via Entity Framework Core)
- **Object Storage:** Azure Blob Storage / Cloudflare R2 (not descided)
### **Frontend**
- **Framework:** React (Vite + TypeScript)
- **UI & Styling:** Tailwind CSS / shadcn/ui
- **Forms:** React Hook Form + Zod
- **State:** Tanstack query
---
 
## 🗄 Core Domain Entities
 
```text
[ Capilla / Center ] ──< [ Group ] ──< [ Student ] ──< [ Document ]
                                            │
                                            ├──< [ Sacrament ]
                                            └──< [ Tutor ]
```
 
---
 
## 🎯 Core Engineering Principles
 
To ensure this project remains sustainable, easy to hand off, and practically useful for parish administration, all development follows four core tenets:
 
- **Remove Friction:** Prioritize UX speed above all else. Whether it's high-speed keyboard navigation (`Tab` + `Enter`) for manual entry, batch processing for physical forms, or mobile-first QR enrollment for parents, the system must save time—never add administrative overhead.
- **Near-Zero Running Cost:** Designed to run comfortably within free/low-cost tiers (PostgreSQL on Supabase/Neon, Cloudflare R2 for zero-egress blob storage, Vercel/Render hosting). Operational costs should remain virtually $0 for the parish.
- **Simplicity Over Complexity:** Resist over-engineering. Favor straight-forward EF Core queries, lightweight API contracts, and clear UI components over complex microservice abstractions or unnecessary dependencies.
- **Gradual Digitalization:** Accommodate real-world constraints. The system seamlessly supports a hybrid environment where paper forms, OCR ingestion, and direct online registration coexist without breaking workflows.
---
 
## 📁 Project Structure
 
```text
guadalupe digital/
├── backend/
│   ├── Features/
│   │   ├── Students/             # Controllers, DTOs, Queries/Services for Students
│   │   └── Ingestion/            # Document ingestion pipelines & extraction strategies
│   ├── Data/
│   │   ├── AppDbContext.cs       # EF Core context & SnakeCase configuration
│   │   └── Migrations/           # Database migrations
│   ├── Common/                   # Shared helpers, middlewares, global filters
│   ├── Program.cs
│   └── GuadalupeDigital.csproj
├── frontend/                     # React (Vite + TypeScript)
└── docs/                         # Diocese specs, API definitions
``` 

## ⚙️ Getting Started
 
### Prerequisites
 
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) and `pnpm` (or `npm`)
- A PostgreSQL instance (local, or free tier via [Supabase](https://supabase.com) / [Neon](https://neon.tech))
- Blob storage credentials (Cloudflare R2 or Azure Blob Storage — either works, storage client is abstracted)
### Installation
 
```bash
# Clone the repo
git clone https://github.com/JuanP-VT/guadalupe-digital.git
cd guadalupe-digital
 
# Backend setup
cd backend
dotnet restore
dotnet ef database update --project src/GuadalupeDigital.Infrastructure
dotnet run --project src/GuadalupeDigital.Api
 
# Frontend setup (in a new terminal)
cd frontend
pnpm install
pnpm dev
```
 
### Environment Variables
 
Create a `.env` (frontend) and `appsettings.Development.json` (backend) based on the examples below:
 
```bash
# backend/appsettings.Development.json (key values)
ConnectionStrings__DefaultConnection="Host=localhost;Database=digital_guadalupe;Username=postgres;Password=postgres"
BlobStorage__Provider="CloudflareR2"   # or "AzureBlob"
BlobStorage__AccountId="<your-account-id>"
BlobStorage__AccessKey="<your-access-key>"
BlobStorage__BucketName="digital-guadalupe-docs"
```
 
```bash
# frontend/.env
VITE_API_BASE_URL="http://localhost:5000/api"
```
 
> ⚠️ Never commit real credentials. Keep `.env` and `appsettings.Development.json` in `.gitignore`.
 
---
 
## 🗺️ Roadmap
 
- [ ] **Epic 1: Foundation & Core Infrastructure** — DB schema, EF Core migrations, Blob Storage client, API/UI scaffolds
- [ ] **Epic 2: Ingestion Pipeline** — Image uploads, OCR Vision processing, split-screen validation UI, DB ingestion
- [ ] **Epic 3: Admin Dashboard & Diocesan Reporting** — Student directory, document viewer, automatic stats generation, Excel export
- [ ] **Epic 4: Public Online Enrollment** — Mobile-first registration wizard, public document uploads, admin approval queue
---
 
## 🤝 Contributing
 
This project is currently developed and maintained as an independent volunteer effort for the parish. If you'd like to contribute (dev, design, or parish-domain expertise), open an issue or reach out directly.
 
---
 
## 📄 License
 
MIT — free to use and adapt for other parishes or dioceses facing the same paper-archive problem.
 
