# SCMS Environment Configuration Guide

This document describes how secrets and environment-specific settings are managed across dev, staging, and production.

## Environment Files

| File | Used When | Secret-safe? |
|------|-----------|-------------|
| `appsettings.json` | All environments (base/defaults) | ✅ Yes — no real secrets |
| `appsettings.Development.json` | `ASPNETCORE_ENVIRONMENT=Development` | ✅ Dev-only keys, not committed to prod |
| `appsettings.Production.json` | `ASPNETCORE_ENVIRONMENT=Production` | ✅ Tokens replaced by CI/CD |

## Required Secrets (Production)

Set these as **environment variables** or **CI/CD pipeline secrets** — never hardcode in committed files.

| Token | Description | Example |
|-------|-------------|---------|
| `SCMS_API_BASE_URL` | Compose value forwarded to the web app's `ApiSettings__BaseUrl` | `http://api:8080` |
| `ApiSettings__BaseUrl` | API base URL used by the web app when configured outside Compose | `https://api.example.edu` |
| `SCMS_DB_CONNECTION_STRING` | SQL Server production connection string | `Server=prod-sql;Database=SCMSDb;User Id=sa;Password=...` |
| `SCMS_JWT_KEY` | JWT signing key (min 32 chars, random) | Generate with: `openssl rand -base64 32` |
| `SCMS_UPLOAD_PATH` | Absolute path for file uploads | `/var/scms/uploads` |
| `SCMS_ADMIN_EMAIL` | Initial admin seed email | `admin@university.edu` |
| `SCMS_ADMIN_PASSWORD` | Initial admin seed password | Strong password, min 8 chars |
| `SCMS_ADMIN_USERNAME` | Initial admin username | `sysadmin` |
| `SCMS_ADMIN_FULLNAME` | Initial admin display name | `System Administrator` |

## Docker / docker-compose

Secrets are passed via `environment:` in `docker-compose.yml`:

```yaml
api:
  environment:
    ASPNETCORE_ENVIRONMENT: "Production"
    ConnectionStrings__DefaultConnection: "${SCMS_DB_CONNECTION_STRING}"
    Jwt__Key: "${SCMS_JWT_KEY}"
```

Set them in a `.env` file (never commit this):
```env
SCMS_DB_CONNECTION_STRING=Server=...
SCMS_JWT_KEY=...
SCMS_API_BASE_URL=http://api:8080
ApiSettings__BaseUrl=https://api.example.edu
```

## Local Development

For local dev, use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```bash
cd src/SCMS.API
dotnet user-secrets set "Jwt:Key" "your-local-dev-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-local-db"
```

The Development API connection string defaults to the SQL Server published by `docker compose` on `localhost,1433`. The web app requires `ApiSettings:BaseUrl`; it has no localhost fallback. Docker Compose sets this to `http://api:8080` for the web container.

## .gitignore Rules

The following are already git-ignored and must never be committed:
- `.env`
- `appsettings.*.local.json`
- `secrets.json`

## Staging

Staging uses `ASPNETCORE_ENVIRONMENT=Staging` and its own secrets injected via the CI pipeline.
Add an `appsettings.Staging.json` mirroring Production if staging-specific overrides are needed.
