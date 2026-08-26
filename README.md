# Digital Guadalupe ⛪

**Digital Guadalupe** is a web-based administration platform designed to digitize, streamline, and manage student enrollments, document archives, and diocesan metrics for the **Parroquia Nuestra Señora de Guadalupe** in Juan José Ríos, Sinaloa.

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
- **Framework:** .NET 8 (C#) Web API
- **Database:** PostgreSQL / SQL Server (ORM via Entity Framework Core)
- **Object Storage:** Azure Blob Storage / Cloudflare R2 (for document storage)

### **Frontend**
- **Framework:** React (Vite + TypeScript)
- **UI & Styling:** Tailwind CSS / shadcn/ui
- **State & Forms:** React Hook Form + Zod

---

## 🗄 Core Domain Entities

```text
[ Capilla / Center ] ──< [ Group ] ──< [ Student ] ──< [ Document ]
                                            │
                                            ├──< [ Sacrament ]
                                            └──< [ Tutor ]
