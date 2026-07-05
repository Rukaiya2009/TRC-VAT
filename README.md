# TRC VAT Risk Checker – Backend API

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)  
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-green)  
![License](https://img.shields.io/badge/License-Proprietary-red)

**TRC VAT Risk Checker** is an ASP.NET Core Web API that powers the VAT audit and risk‑assessment platform for TRC (a Bangladesh‑based VAT consultancy). It handles:

- ✅ **Customs duty & VAT calculation** – accurately reproduces the math from a real Bangladesh Bill of Entry.
- ✅ **12‑rule risk engine** – configurable, client‑approved rules that score each import (Low/Medium/High).
- ✅ **Multi‑channel data entry** – manual forms, Excel upload, and (later) BOE PDF extraction.
- ✅ **Appointment booking** – fixed daily window (4–6 PM) with equal time division, cancellation, and no‑show blocking.
- ✅ **Bilingual support** – English and বাংলা (Bangla) everywhere, from UI labels to notification templates.
- ✅ **OTP‑secured reports** – phone‑verified access to risk results and download‑able calculation sheets.

The API is designed with **Clean Architecture** (Domain, Application, Infrastructure, Shared, API) and is fully containerised for deployment on **Render**. The database is hosted on **Supabase** (PostgreSQL) and the frontend (separate repository) is built with **React + TypeScript** and deployed on **Vercel**.

---

## 📁 Repository Structure

```bash
.
├── TRC.API/                  # Controllers, middleware, DTOs, Program.cs
├── TRC.Application/          # Services, interfaces, DTOs, validators, AutoMapper
├── TRC.Domain/               # Entities, enums, repository interfaces (pure C#)
├── TRC.Infrastructure/       # DbContext, migrations, repositories, external providers
├── TRC.Shared/               # Constants, helpers, resource files (en/bn)
├── TRC-VAT-Risk-Checker.sln  # Solution file
├── Dockerfile                # Container build for Render
├── .gitignore
└── README.md
```

> The frontend (`web/`) lives in a **separate repository** – see [TRC-VAT-Web](https://github.com/Rukaiya2009/TRC-VAT-Web) (placeholder).

---

## ⚙️ Technology Stack

| Layer          | Technology |
|----------------|------------|
| **Runtime**    | .NET 8 LTS (ASP.NET Core Web API) |
| **Database**   | Supabase (PostgreSQL 15+) + EF Core (Npgsql) |
| **ORM**        | Entity Framework Core 8.x |
| **Auth**       | JWT Bearer (ASP.NET Core Identity primitives) |
| **Validation** | FluentValidation |
| **Mapping**    | AutoMapper |
| **API Docs**   | Swagger / OpenAPI (Swashbuckle) |
| **Excel**      | EPPlus |
| **PDF**        | QuestPDF |
| **Testing**    | xUnit + Moq + FluentAssertions + Testcontainers |
| **Notifications** | Abstraction over WhatsApp (Meta/Twilio) & Email (HTTP API) |
| **Localization** | ASP.NET Core resource files (`.resx`) |
| **Hosting**    | Render (Docker container) – API, Vercel – frontend, Supabase – DB |

---

## 🚀 Getting Started (Development)

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (or VS Code + C# extension)
- [Supabase account](https://supabase.com/) – or a local PostgreSQL instance (but Supabase is recommended)
- [Git](https://git-scm.com/)

### Clone the repository

```bash
git clone https://github.com/Rukaiya2009/TRC-VAT.git
cd TRC-VAT/TRC-VAT-Risk-Checker
```

### Set up User Secrets (for local development)

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=db.xxxxxx.supabase.co;Database=postgres;Username=postgres;Password=your_supabase_password;Port=5432;SslMode=Require"
dotnet user-secrets set "Jwt:Key" "your_super_secret_jwt_key_at_least_32_chars"
dotnet user-secrets set "Jwt:Issuer" "TRC"
dotnet user-secrets set "Jwt:Audience" "TRC"
```

> Use the **session/direct** connection string (port **5432**) for migrations.  
> The pooled connection (port 6543) is for runtime traffic – we’ll use it in production.

### Apply database migrations

```bash
dotnet ef database update --project TRC.Infrastructure --startup-project TRC.API
```

This will create the schema and seed the `RiskRule` table with the 12 approved rules.

### Run the API

```bash
cd TRC.API
dotnet run
```

The API will be available at `https://localhost:7171` (or `http://localhost:5236`).  
Swagger UI is at `/swagger`.

---

## 🧪 Testing

```bash
dotnet test
```

Unit tests cover:
- Tax calculation (against the reference Bill of Entry)
- Each risk rule
- Appointment slot division
- OTP rate limiting
- Localization fallback

Integration tests (with Testcontainers) run against a real PostgreSQL container.

---

## 🐳 Docker (for Render deployment)

Build the image:

```bash
docker build -t trc-api .
```

Run locally:

```bash
docker run -p 8080:80 -e ConnectionStrings__DefaultConnection="Host=..." -e Jwt__Key="..." trc-api
```

The `Dockerfile` is multi‑stage and optimised for production.

---

## 📬 Environment Variables (Production)

On Render, set these environment variables:

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Supabase pooled connection string (port 6543) |
| `Jwt__Key` | 32+ char secret |
| `Jwt__Issuer` | `TRC` |
| `Jwt__Audience` | `TRC` |
| `WhatsApp__Provider` | `Meta` or `Twilio` |
| `WhatsApp__ApiKey` | (if applicable) |
| `Email__Provider` | `ZeptoMail` or `SendGrid` |
| `Email__ApiKey` | |
| `Email__FromAddress` | e.g. `noreply@trc.com` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

---

## 📋 Key Endpoints (Summary)

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/auth/login` | Login with email/password → JWT |
| `POST` | `/api/auth/register` | Register a new user (Admin only) |
| `POST` | `/api/otp/send` | Send 6‑digit OTP to phone |
| `POST` | `/api/otp/verify` | Verify OTP |
| `GET`  | `/api/imports` | List all imports |
| `POST` | `/api/imports` | Create a new import (triggers tax calc) |
| `POST` | `/api/imports/upload` | Upload Excel file with imports |
| `POST` | `/api/imports/{id}/assess` | Run risk assessment |
| `GET`  | `/api/imports/{id}/risk` | Get latest risk result |
| `GET`  | `/api/reports/import/{id}` | Download full report (JSON/PDF) |
| `GET`  | `/api/reports/calculation-sheet` | Download calculation sheet PDF |
| `POST` | `/api/appointments` | Book a consultation slot (OTP required) |
| `GET`  | `/api/consultation-days` | List available dates |

> Full OpenAPI documentation is available via Swagger after launching the API.

---

## 📊 Data Model Highlights

- **Import** – declaration data, HS code, invoice, exchange rate, computed AV and total tax.
- **TaxBreakdown** – one record per tax type (CD, RD, SD, VAT, AIT, AT, ATV).
- **BusinessPeriodData** – monthly sales/purchases/VAT (used by risk rules R1 and R6).
- **RiskRule** – configurable 12 rules with weights, thresholds, enabled flag.
- **RiskAssessment** – per‑import score, level, list of triggered rules (JSON).
- **PhoneProfile** – tracks OTP verification, missed appointments, block status.
- **Appointment** – booking details with assigned start/end times (determined at cutoff).

For a full Entity Relationship Diagram, see the [SRS document](https://github.com/Rukaiya2009/TRC-VAT/blob/main/docs/SRS.md).

---

## 🤝 Contributing

This project is developed **internally** for TRC.  
If you are a team member:

- Branch off `main` with `feature/` or `fix/` prefixes.
- Write unit tests for new services.
- Update the SRS if you change functional behaviour.
- Use conventional commits.

---

## 📄 License

Proprietary – all rights reserved.  
© 2026 TRC VAT Consultancy. Unauthorised copying or distribution is prohibited.

---

## 📞 Support

For questions or issues, contact the development team:  
**Rukaiya Binte Shafique** – [rukaiyabinte2009@gmail.com](mailto:rukaiyabinte2009@gmail.com)

---

**Happy auditing!** 🧾
