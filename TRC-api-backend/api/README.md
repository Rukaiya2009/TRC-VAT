# TRC VAT Risk Checker — Backend (`/api`)

ASP.NET Core 8 Web API, Clean Architecture. Part of the TRC monorepo (`/api` + `/web`).

## Projects
| Project | Layer | Responsibility |
|---|---|---|
| `TRC.Domain` | Core | Entities, enums, repository interfaces. No dependencies. |
| `TRC.Application` | Use cases | Services (Tax, Variance, Risk, Import, Auth), DTOs, interfaces, validators. |
| `TRC.Infrastructure` | External | EF Core + Npgsql (Supabase), repositories, JWT, password hasher, notifications. |
| `TRC.Shared` | Cross-cutting | `ApiResponse<T>`, constants. |
| `TRC.API` | Presentation | Controllers, middleware, Swagger, JWT, CORS, `Program.cs`. |
| `TRC.Tests` | Tests | TC-4.1 tax acceptance test against the Bill of Entry. |

Dependency flow: `API → Application → Domain` · `Infrastructure → Domain` (implements Application interfaces) · `Shared → all`.

## First run
```bash
cd api
dotnet restore
dotnet user-secrets init --project TRC.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=db.xxxx.supabase.co;Database=postgres;Username=postgres;Password=...;Port=5432;SslMode=Require" --project TRC.API
dotnet user-secrets set "Jwt:Key" "a-32+char-secret-key-goes-right-here" --project TRC.API

# migrations use the direct/session connection (port 5432)
dotnet ef migrations add InitialCreate --project TRC.Infrastructure --startup-project TRC.API
dotnet ef database update --project TRC.Infrastructure --startup-project TRC.API

dotnet run --project TRC.API   # Swagger at /swagger
dotnet test                    # runs TC-4.1
```
`dotnet ef` reads the connection from the `TRC_MIGRATIONS_CONNECTION` env var if set (see `DesignTimeDbContextFactory`); otherwise User Secrets on the API project.

## Built in this scaffold (Phases 0–3 foundation)
- All 14 domain entities + `AppDbContext` with the **12 approved risk rules seeded**.
- **Tax engine** (Section 8) + TC-4.1 acceptance test.
- **Variance quick-check** (§8.2) with configurable gauge thresholds.
- **Risk engine** running Phase A rules (R5, R7, R10, R11) with Not-Evaluable routing for Phase B.
- **JWT auth** (register/login/refresh), role authorization, global exception middleware, Swagger Authorize.
- Endpoints: `/api/auth/*`, `/api/imports` (+`/assess`), `/api/quick-checks`, `/api/risk-rules`, `/api/health`.

## Next (per roadmap)
Phase 4 — OTP, ConsultationDay/Appointments (cutoff division), real notification providers.
Phase 5 — Excel upload (EPPlus), calculation-sheet PDF (QuestPDF), raw HTML test client, Render deploy.
