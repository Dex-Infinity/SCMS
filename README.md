# Student Complaint Management System (SCMS)

A web-based platform for submitting, tracking, and resolving student complaints within a university environment — replacing manual paper/email-based processes with a centralized, transparent system.

## Tech Stack

- **Backend:** ASP.NET Core Web API
- **Frontend:** Blazor Web Application
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Auth:** ASP.NET Identity

## Folder Structure

```
SCMS/
├── .github/
│   └── workflows/
│       └── ci.yml                  # GitHub Actions CI pipeline
│
├── src/
│   ├── SCMS.API/                   # ASP.NET Core Web API (Backend)
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Repositories/
│   │   ├── Middleware/
│   │   ├── DTOs/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── SCMS.Web/                   # Blazor Frontend
│   │   ├── Pages/
│   │   │   ├── Student/
│   │   │   └── Admin/
│   │   ├── Shared/                 # Layout, NavMenu, shared components
│   │   ├── Services/                # API client services
│   │   └── wwwroot/
│   │
│   ├── SCMS.Domain/                # Entities & core business models
│   │   ├── Entities/
│   │   └── Enums/
│   │
│   └── SCMS.Infrastructure/        # EF Core, migrations, identity
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── Migrations/
│       └── Identity/
│
├── tests/
│   ├── SCMS.API.Tests/              # Backend unit/integration tests
│   └── SCMS.Web.Tests/              # Frontend component tests
│
├── docs/
│   ├── wireframes/                  # UI/UX design exports
│   └── er-diagram.png               # Database schema diagram
│
├── .gitignore
├── CONTRIBUTING.md
└── README.md
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 / VS Code

### Setup
```bash
# Clone the repo
git clone https://github.com/<org>/SCMS.git
cd SCMS

# Restore dependencies
dotnet restore

# Apply database migrations
cd src/SCMS.Infrastructure
dotnet ef database update

# Run the API
cd ../SCMS.API
dotnet run

# Run the Blazor frontend (in a separate terminal)
cd ../SCMS.Web
dotnet run
```

## Branching Strategy

- `main` — stable, deployable code only
- `develop` — integration branch for completed features
- `feature/<name>` — individual task branches, branched off `develop`

All changes go through a pull request into `develop`, with at least one review before merging. See `CONTRIBUTING.md` for details.

## Team

| Role | Members |
|---|---|
| Project Lead | Nana Kofi Agyin |
| Backend Developer | Virtus Dakura, Amartey Felix Laryea |
| Frontend Developer | Irene Darah-Mensah, Elikplim Yevu |
| UI/UX Designer | Roselyn Francis, Quartey Obed Nii Kpakpa |
| Database Engineer | Jessica Zunuo Puozaa, Seglah Emmanuel, Collins Edumadze Egyir |
| DevOps Engineer | Keren Asabea Acquaah, Rushdan Delimwine Antiku |

## Key Features

1. **Complaint Submission** — students submit complaints with supporting document attachments
2. **Complaint Tracking** — real-time status updates for students
3. **Complaint Management** — admins review, assign, update, and resolve complaints
4. **Notifications** — status-change alerts for users
5. **Reports & Analytics** — complaint statistics and performance dashboards for management

## License

Academic project — for coursework purposes.
