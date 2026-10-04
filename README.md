# Student Complaint Management System (SCMS)

A web-based platform for submitting, tracking, and resolving student complaints within a university environment — replacing manual paper/email-based processes with a centralized, transparent system.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend API** | ASP.NET Core 10 Web API |
| **Frontend** | Blazor Web Application (.NET 10) |
| **ORM** | Entity Framework Core 10 |
| **Database** | SQL Server 2022 |
| **Auth** | ASP.NET Identity + JWT Bearer |
| **Containerization** | Docker + Docker Compose |

## Key Features

1. **Complaint Submission** — students submit complaints with supporting document attachments
2. **Complaint Tracking** — real-time status updates with a progress stepper (Submitted → Under Review → Assigned → Resolved)
3. **Complaint Management** — admins review, assign to departments, and resolve or reject complaints
4. **Status History** — full audit trail of every complaint status transition
5. **Notifications** — in-app alerts triggered on every status change
6. **Reports & Analytics** — complaint statistics by status and department, resolution time metrics
7. **Attachments** — file upload/download support (PDF, JPG, PNG, DOC, DOCX)

## Folder Structure

```
SCMS/
├── .github/
│   └── workflows/
│       └── ci.yml                  # GitHub Actions CI pipeline
│
├── src/
│   ├── SCMS.API/                   # ASP.NET Core Web API (Backend)
│   │   ├── Controllers/            # AuthController, ComplaintsController, etc.
│   │   ├── Services/               # Business logic (Auth, Complaint, Analytics, etc.)
│   │   ├── Repositories/           # Data access (Complaint, Analytics, Attachment)
│   │   ├── Middleware/             # Exception handling middleware
│   │   ├── DTOs/                   # Request/Response data transfer objects
│   │   ├── Program.cs
│   │   ├── appsettings.json        # Base config (all environments)
│   │   ├── appsettings.Development.json
│   │   └── appsettings.Production.json
│   │
│   ├── SCMS.Web/                   # Blazor Frontend
│   │   ├── Pages/
│   │   │   ├── Student/            # SubmitComplaint, TrackComplaints, ComplaintDetails
│   │   │   └── Admin/              # AdminDashboard, AnalyticsDashboard
│   │   ├── Shared/                 # Layout, NavMenu, shared components
│   │   ├── Services/               # API client services
│   │   └── wwwroot/
│   │
│   ├── SCMS.Domain/                # Entities & core business models
│   │   ├── Entities/               # Complaint, Student, Department, Admin,
│   │   │                           # Attachment, Notification, StatusHistory, UserSetting
│   │   └── Enums/                  # ComplaintStatus
│   │
│   └── SCMS.Infrastructure/        # EF Core, migrations, identity
│       ├── Data/
│       │   └── ApplicationDbContext.cs
│       ├── Migrations/             # EF Core migration history
│       └── Identity/               # ApplicationUser, ApplicationRole, DbInitializer
│
├── tests/
│   ├── SCMS.API.Tests/             # Backend unit/integration tests
│   └── SCMS.Web.Tests/             # Frontend component tests
│
├── docs/
│   ├── wireframes/                 # UI/UX design exports
│   │   ├── user-flows/             # student-user-flow.png, admin-user-flow.png
│   │   ├── student-dashboard.png
│   │   ├── reporting-dashboard.png
│   │   └── admin-dashboard/
│   ├── er-diagram.png              # Full database ER diagram
│   ├── TASKS.md                    # Team task assignments & status
│   ├── DEPLOYMENT.md               # Deployment & runbook guide
│   ├── DEVELOPMENT_GUIDE.md        # Developer onboarding guide
│   └── ENVIRONMENT_CONFIG.md       # Secrets & env config guide
│
├── docker-compose.yml
├── .gitignore
├── CONTRIBUTING.md
└── README.md
```

## Getting Started

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) *(recommended — no other installs needed)*
- **OR** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) + SQL Server (for manual run)

---

### ▶ Option 1: Run with Docker (Recommended — One Command)

Docker spins up the **API**, **Blazor frontend**, and **SQL Server** automatically with health-checked startup ordering.

```bash
# Clone the repo
git clone https://github.com/Dex-Infinity/SCMS.git
cd SCMS

# Build and start all services
docker compose up --build
```

> First boot takes ~60–90 seconds while SQL Server initialises and the API runs migrations.

Once running, open your browser:

| Service | URL |
|---------|-----|
| 🌐 **Web App (Blazor)** | http://localhost:8081 |
| ⚙️ **API (Swagger UI)** | http://localhost:8080/swagger |
| 🗄️ **SQL Server** | `localhost,1433` · user: `sa` · password: `SCMS@SqlServer2026!` |

To stop all services:
```bash
docker compose down
```

---

### ▶ Option 2: Run Manually with .NET CLI

**1. Clone and restore:**
```bash
git clone https://github.com/Dex-Infinity/SCMS.git
cd SCMS
dotnet restore
```

**2. Apply database migrations** *(requires SQL Server or LocalDB)*:
```bash
dotnet ef database update --project src/SCMS.Infrastructure --startup-project src/SCMS.API
```

> If you don't have SQL Server locally, skip this step. The API will fall back to an **in-memory database** automatically.

**3. Run the API** *(Terminal 1)*:
```bash
cd src/SCMS.API
dotnet run
```
API: **http://localhost:5000** · Swagger: **http://localhost:5000/swagger**

**4. Run the Frontend** *(Terminal 2)*:
```bash
cd src/SCMS.Web
dotnet run
```
Web app: **http://localhost:5001**

---

## API Reference

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/auth/register` | Public | Register new student |
| `POST` | `/api/auth/login` | Public | Login, returns JWT token |
| `GET` | `/api/complaints` | JWT | Get all complaints |
| `POST` | `/api/complaints` | JWT | Submit new complaint |
| `GET` | `/api/complaints/{id}` | JWT | Get complaint by ID |
| `PUT` | `/api/complaints/{id}/status` | JWT | Update complaint status |
| `PUT` | `/api/complaints/{id}/assign` | JWT | Assign to department/admin |
| `GET` | `/api/reports/summary` | JWT | Analytics summary |
| `GET` | `/api/notifications` | JWT | Get user notifications |
| `POST` | `/api/attachments` | JWT | Upload file attachment |

Full interactive docs available at **http://localhost:8080/swagger**.

## Branching Strategy

- `main` — stable, deployable code only
- `feat/<name>` — individual task/feature branches

All changes go through a Pull Request into `main` with at least one review. See [CONTRIBUTING.md](../CONTRIBUTING.md) for details.

## Database Schema

See [docs/er-diagram.png](./docs/er-diagram.png) for the full ER diagram.

**Core tables:** `Students`, `Departments`, `Admins`, `Complaints`, `Attachments`, `StatusHistory`, `Notifications`, `UserSettings`, `AspNetUsers` (Identity)

## Team

| Role | Members |
|------|---------|
| Project Lead | Nana Kofi Agyin |
| Backend Developer | Virtus Dakura, Amartey Felix Laryea |
| Frontend Developer | Irene Darah-Mensah, Elikplim Yevu |
| UI/UX Designer | Roselyn Francis, Quartey Obed Nii Kpakpa |
| Database Engineer | Jessica Zunuo Puozaa, Seglah Emmanuel, Collins Edumadze Egyir |
| DevOps Engineer | Keren Asabea Acquaah, Rushdan Delimwine Antiku |

## License

Academic project — for coursework purposes.
