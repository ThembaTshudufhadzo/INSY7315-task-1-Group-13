# UbuhleBethu

# UbuhleBethu ConnectPro (Web)

An ASP.NET Core MVC web application for Ubuhlebethu, a construction company. It connects clients, office/QS staff, site managers, and management/admin around a single workflow: a client requests a quote, office staff review and price it, an invoice is generated, and management gets visibility into projects and reporting — all with role-based dashboards and an audit trail.

## Features

- **Authentication & roles** — ASP.NET Core Identity with cookie-based login, seeded roles: `Admin`, `Office`, `Client`, `SiteManager`, `Management` (plus `Executive` on some dashboard views).
- **Quotations** — clients submit a quote request (project type, site location, budget, description); office staff review and approve it; clients can mark approved quotes as paid.
- **Invoicing** — preview and download invoices generated from approved quotations.
- **Role-specific dashboards**:
  - **Office** — pending quotes queue, draft estimate generation, sending invoices.
  - **Client Portal** — a client's own quote/project history, filterable by date and status.
  - **Executive / Management** — high-level reporting.
  - **Admin** — user management (search/filter, deactivate/reactivate, edit roles, export), audit log viewing and export, live stats, portfolio view, regional map, reports.
- **Notifications** — in-app notification list with mark-as-read.
- **Audit logging** — admin actions are recorded and viewable/exportable from the Admin dashboard.
- **Seed data** — on first run, the app seeds demo roles, one sample user per role, and a handful of realistic mock quote requests so the app is immediately explorable.

## Tech Stack

- **Framework:** ASP.NET Core MVC, .NET 10
- **ORM / Data:** Entity Framework Core 8 with SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.Data.SqlClient`)
- **Auth:** ASP.NET Core Identity (`Microsoft.AspNetCore.Identity.EntityFrameworkCore`)
- **Other:** Azure.Identity

## Project Structure

```
Controllers/
├── AccountController.cs        # Login / logout / access denied
├── AdminController.cs          # User management, audit logs
├── DeshboardController.cs      # DashboardController: Office/Client/Executive/Admin/Management views
├── HomeController.cs           # Landing page, privacy, error
├── InvoiceController.cs        # Invoice preview & download
├── ManagerController.cs        # Management dashboard
├── NotificationsController.cs  # Notification list & mark-as-read
├── OfficeController.cs         # Pending quotes, estimates, invoicing, approvals
└── QuotationController.cs      # Client quote request creation, review, approval, payment

Models/            # QuoteRequest, Invoice, Payment, Project, Job, Notification, AuditLog, ...
Data/               # ApplicationDbContext
Migrations/         # EF Core migrations (Identity, AuditLog, Project entity)
Views/              # Razor views, grouped by controller
wwwroot/            # Static assets (css, js, images, lib)
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB, SQL Express, or full SQL Server)
- Visual Studio 2022+ or the `dotnet` CLI

### Setup

1. **Configure the database connection.**
   Update the `ConnectionStrings:DefaultConnection` value in `appsettings.json` (and/or `appsettings.Development.json`) to point at your own SQL Server instance — the checked-in value points at a specific lab machine and won't work on other setups:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ConnectProDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

2. **Restore and run** — migrations are applied automatically on startup (`db.Database.Migrate()`), and demo roles, users, and sample quote requests are seeded the first time the app runs:
   ```bash
   dotnet restore
   dotnet run
   ```
   Or open `UbuhlebethuConnectPro.Web.slnx` in Visual Studio and press Run.

3. Browse to the URL shown in the console (see `Properties/launchSettings.json`).

### Seeded demo accounts

| Username | Password | Role |
|---|---|---|
| admin | admin123 | Admin |
| office | office123 | Office |
| client | client123 | Client |
| manager | manager123 | Management |
| site | site123 | SiteManager |

> These are demo/prototype credentials only — rotate or remove the seeding logic in `Program.cs` before any real deployment, and don't reuse these passwords outside local development.

## Security Notes

- Password policy is intentionally relaxed in `Program.cs` for prototype convenience (no digit/uppercase/non-alphanumeric requirement, 6-character minimum). Tighten this before production use.
- The default connection string in source control targets a specific developer machine — replace it with your own and avoid committing real credentials or production connection strings.

## Roadmap Ideas

- Harden the Identity password policy and remove hardcoded seed credentials for production.
- Move the connection string to user secrets / environment variables rather than `appsettings.json`.
- Connect to (or replace) the companion [UbuhlebethuMApp Android app](#) so field/site data flows into the same backend.
- Expand automated test coverage.
