# SCMS Deployment & Runbook

This document outlines how to deploy, run, and maintain the SCMS (Student Complaint Management System) stack.  
For secrets and environment-specific config, see [ENVIRONMENT_CONFIG.md](./ENVIRONMENT_CONFIG.md).

---

## Stack Overview

| Service | Container | Port |
|---------|-----------|------|
| PostgreSQL 17 | `scms-postgres` | `5432` |
| SCMS REST API | `scms-api` | `8080` |
| SCMS Blazor Web App | `scms-web` | `8081` |

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- Git (to clone the repo)

---

## Running Locally with Docker Compose

### Quick Start

```bash
# 1. Clone the repo
git clone https://github.com/Dex-Infinity/SCMS.git
cd SCMS

# 2. Build and start the full stack
docker compose up --build
```

Once all containers are healthy, access the apps at:

| App | URL |
|-----|-----|
| **Swagger UI** | http://localhost:8080/swagger |
| **Web App** | http://localhost:8081 |
| **PostgreSQL** | `localhost:5432` (local-only credentials in `docker-compose.yml`) |

> **Note**: The API waits for SQL Server to pass a health check before starting.  
> The Web app waits for the API to pass its health check before starting.  
> First boot takes ~60–90 seconds.

### Startup Health Check Order

```
scms-sqlserver  →  (healthy: sqlcmd SELECT 1)
       ↓
scms-api        →  (healthy: TCP port 8080 accepting connections)
       ↓
scms-web        →  starts
```

### Stopping the Stack

Stop containers (preserves database volume):
```bash
docker compose stop
```

Stop and remove containers + orphans:
```bash
docker compose down --remove-orphans
```

Stop and wipe everything including the database volume:
```bash
docker compose down -v
```

---

## Database Migrations

Migrations run **automatically on API startup** via `MigrateAsync()` — no manual steps needed.

To generate a new migration after schema changes:
```bash
dotnet ef migrations add <MigrationName> \
  --project src/SCMS.Infrastructure \
  --startup-project src/SCMS.API
```

To manually apply migrations:
```bash
dotnet ef database update \
  --project src/SCMS.Infrastructure \
  --startup-project src/SCMS.API
```

---

## Environment Variables

All secrets are injected via environment variables. See [ENVIRONMENT_CONFIG.md](./ENVIRONMENT_CONFIG.md) for the full list.

Key variables used by `docker-compose.yml`:

| Variable | Service | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | api, web | `Development` or `Production` |
| `ConnectionStrings__DefaultConnection` | api | SQL Server connection string |
| `ApiSettings__BaseUrl` | web | Internal API URL (e.g. `http://api:8080`) |
| `MSSQL_SA_PASSWORD` / `ACCEPT_EULA` | sqlserver | SQL Server SA credentials |

---

## Verifying the Deployment

### Check all containers are running
```bash
docker compose ps
```

### Check API is alive
```powershell
Invoke-WebRequest -Uri http://localhost:8080/swagger/v1/swagger.json -UseBasicParsing
# Expect: StatusCode 200
```

### Check the database has the SCMS schema
```bash
docker exec -it scms-postgres psql -U scms -d SCMSDb -c "\\dt"
# Expect: EF tables such as AspNetUsers and Complaints listed
```

### Stream live logs
```bash
docker logs scms-api -f
docker logs scms-web -f
docker logs scms-postgres -f
```

---

## Continuous Integration (CI)

A GitHub Actions pipeline is configured in `.github/workflows/ci.yml`.  
It automatically restores, builds, and tests the solution on every push to `main` or any `feat/*` branch.

---

## Production Deployment

1. Set all required secrets as environment variables on the host or in the CI/CD pipeline (see [ENVIRONMENT_CONFIG.md](./ENVIRONMENT_CONFIG.md))
  Set `ApiSettings__BaseUrl` for the web service to the deployed API's internal HTTPS URL.
  On Render, attach the PostgreSQL database to the API service so `DATABASE_URL` is available, and set `SCMS_JWT_KEY` on the API service.
2. Set `ASPNETCORE_ENVIRONMENT=Production`
3. Run:
   ```bash
   docker compose up -d --build
   ```
4. Migrations will run automatically on first boot
5. The seed admin account is only created if `Seed:SeedOnStartup` is `true` (disabled in production by default)
