# SCMS Deployment & Runbook

This document outlines how to deploy and run the SCMS (Student Complaint Management System) environment. 

## Running Locally with Docker Compose

We use Docker Compose to spin up the entire application stack:
- SQL Server Database
- SCMS API
- SCMS Blazor Web App

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### Quick Start

1. Open your terminal at the root of the `SCMS` repository.
2. Run the following command:
   ```bash
   docker-compose up -d --build
   ```
3. Once running, you can access the applications at:
   - **API**: http://localhost:5000 (Swagger UI at http://localhost:5000/swagger)
   - **Web App**: http://localhost:5001

### Stopping the Environment

To stop the containers without destroying the database data:
```bash
docker-compose stop
```

To stop and remove everything (including the database volume):
```bash
docker-compose down -v
```

## Continuous Integration (CI)

A GitHub Actions pipeline is configured in `.github/workflows/ci.yml`. It automatically restores, builds, and tests the solution on every push to the `main` or `dev` branches.

## Environment Variables

- `MSSQL_SA_PASSWORD`: The SA password for the SQL Server instance (defaulted in `docker-compose.yml`).
- `ConnectionStrings__DefaultConnection`: The connection string passed to the API.
- `ApiSettings__BaseUrl`: The API base URL passed to the Web application.
