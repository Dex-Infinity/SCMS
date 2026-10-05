# SCMS Testing Results (in progress)

Tested on: 5 October 2026, branch feature/testing-and-db-docs

## Environment
- Stack run with docker compose: PostgreSQL 17 (alpine), API on port 8080, web on port 8081
- All containers reported healthy or started
- Note: README says SQL Server and ports 5000/5001; the running setup uses PostgreSQL and 8080/8081

## Automated tests (SCMS.API.Tests)
- dotnet test on Windows: 25 of 25 failed, blocked by a Windows Application Control policy on SCMS.API.dll (environment issue, not a code defect). Status: Blocked
- dotnet test in Docker (.NET 10 SDK image): 25 total, 25 passed, 0 failed. Status: Pass
- Coverage: complaints, notifications, attachments, reports, departments (mostly authorization and not-found checks)
- Not covered: creating a complaint, status changes, assignment, login, duplicate checks, file upload limits
- SCMS.Web.Tests: no tests exist

## Database checks (PostgreSQL, via psql)
- DB-00 Schema created: 16 tables (8 SCMS, 7 Identity, __EFMigrationsHistory), 2 migrations applied. Pass
- DB-01 Unique constraint: duplicate department code rejected (IX_Departments_Code). Pass
- DB-02 Foreign key: complaint for a non-existent student rejected (FK_Complaints_Students_StudentId). Pass
- DB-03 Seed data: 3 departments present (CS, EE, AA). Pass
- DB-04 Indexes: Complaints and Students indexes match the configuration; 6 unique indexes outside Identity. Pass

## Application tests
- T01 Admin login: sign-in failed with a generic error; cause under investigation. Fail (cause unknown)
- T02 to T16: not yet run

## Findings to raise with the team
1. README is out of date: it says SQL Server and ports 5000/5001, but the running system uses PostgreSQL and 8080/8081.
2. Deleting a student cascades to their complaints, attachments and history (database rule).
3. No foreign key links Students or Admins to the login accounts; the Admins table is empty, so complaint assignment is untested.
4. Notifications, user settings and several user id columns have no foreign keys.
5. SCMS.Web.Tests contains no tests.
