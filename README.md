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
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) *(for the Docker option)*
- Visual Studio 2022 / VS Code *(for the manual option)*

---

### ▶ Option 1: Run with Docker (Recommended — One Command)

This is the easiest way to get everything running. Docker will spin up the **API**, **Frontend**, and **SQL Server database** automatically.

```bash
# Clone the repo
git clone https://github.com/Dex-Infinity/SCMS.git
cd SCMS

# Build and start all services
docker-compose up -d --build
```

Once running, open your browser and visit:

| Service | URL |
|---|---|
| 🌐 **Frontend (Blazor Web)** | http://localhost:5001 |
| ⚙️ **Backend API (Swagger UI)** | http://localhost:5000/swagger |
| 🗄️ **SQL Server** | `localhost,1433` (SA password: `SuperSecretPassword123!`) |

To stop all services:
```bash
docker-compose down
```

---

### ▶ Option 2: Run Manually with .NET CLI

Use this option if you prefer to run each service individually for development.

**1. Clone the repo and restore dependencies:**
```bash
git clone https://github.com/Dex-Infinity/SCMS.git
cd SCMS
dotnet restore
```

**2. Apply database migrations** *(requires a local SQL Server or LocalDB instance)*:
```bash
cd src/SCMS.Infrastructure
dotnet ef database update
```

> If you don't have SQL Server, skip this step. The API will automatically fall back to an **in-memory database**.

**3. Run the Backend API** *(in Terminal 1)*:
```bash
cd src/SCMS.API
dotnet run
```
The API will be available at: **http://localhost:5000**
Swagger UI (API docs & testing): **http://localhost:5000/swagger**

**4. Run the Frontend** *(in a new Terminal 2)*:
```bash
cd src/SCMS.Web
dotnet run
```
The web app will be available at: **http://localhost:5001**

> **Note:** The root URL `http://localhost:5000` will return a 404 — this is expected. Always use `/swagger` for the backend API.

## Branching Strategy

- `main` — stable, deployable code only
- `dev` — integration branch for completed features
- `feature/<name>` — individual task branches, branched off `dev`

All changes go through a pull request into `dev`, with at least one review before merging. See `CONTRIBUTING.md` for details.

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
