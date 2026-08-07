# Contributing to SCMS

This document outlines how the team works together in this repository. Please read it before pushing any code.

## Branching Strategy

- `main` — always stable and deployable. No direct pushes.
- `dev` — integration branch where all finished features are merged. No direct pushes.
- `feature/<short-name>` — one branch per task, branched off `dev`.
- `fix/<short-name>` — for bug fixes, branched off `dev`.

### Branch naming examples
```
feature/complaint-submission-api
feature/student-dashboard-ui
feature/database-schema
fix/login-validation-bug
```

## Workflow

1. Pull the latest `dev` before starting any work:
   ```bash
   git checkout dev
   git pull origin dev
   ```
2. Create your feature branch:
   ```bash
   git checkout -b feature/your-task-name
   ```
3. Make your changes, committing in small, meaningful chunks (see commit style below).
4. Push your branch:
   ```bash
   git push origin feature/your-task-name
   ```
5. Open a Pull Request (PR) into `dev` on GitHub.
6. Request a review from at least one teammate — ideally someone working on a related part of the system.
7. Address review comments, then merge once approved. CI must pass before merging.
8. Delete the feature branch after merging.

## Commit Message Style

Keep commits small and descriptive:
```
feat: add complaint submission endpoint
fix: correct status enum mapping in complaint DTO
style: format student dashboard layout
docs: update README setup instructions
```

Prefixes to use: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`.

## Pull Request Guidelines

- Keep PRs focused on a single task — avoid bundling unrelated changes.
- Write a clear PR description: what changed and why.
- Link the related task/issue if using GitHub Projects.
- Make sure the CI build and tests pass before requesting review.
- Resolve all review comments before merging.

## Running the Project Locally

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB is fine for development)
- Visual Studio 2022 or VS Code with C# extension

### Steps
```bash
# Restore dependencies
dotnet restore

# Apply database migrations
cd src/SCMS.Infrastructure
dotnet ef database update

# Run the API (from src/SCMS.API)
dotnet run

# Run the Blazor frontend (from src/SCMS.Web, in a separate terminal)
dotnet run
```

## Code Style

- Follow standard C# naming conventions (PascalCase for classes/methods, camelCase for local variables).
- Keep controllers thin — business logic belongs in the Services layer.
- Add XML doc comments on public API methods where behavior isn't obvious.
- Write unit tests for new service/repository logic where practical.

## Questions

If you're unsure about a task, schema change, or how something should connect to another part of the system (e.g. Backend ↔ Database, Frontend ↔ Backend), raise it with the relevant pair or the project lead before building on assumptions.