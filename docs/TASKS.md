# SCMS Task Assignments

Reference list of tasks per team member. For live tracking, mirror these as GitHub Issues on the Projects board (see below).

## Backend Developer

**Virtus Dakura**
- [ ] Set up the ASP.NET Core Web API project structure (Controllers, Services, Repositories, DTOs)
- [ ] Build Complaint endpoints — create, get by student, get by ID, update status, assign to department
- [ ] Build Notification service — triggers updates on status change
- [ ] Write Swagger/OpenAPI docs for the API

**Amartey Felix Laryea**
- [ ] Implement ASP.NET Identity — registration, login, student/admin roles, JWT/cookie auth
- [ ] Build Attachment handling — upload/download supporting documents
- [ ] Build Reports/Analytics endpoints — counts by status, department, resolution time
- [ ] Add validation and centralized error handling

## Frontend Developer

**Irene Darah-Mensah**
- [ ] Set up the Blazor project and shared layout (nav, auth-aware routing)
- [ ] Build the admin dashboard (queue, filters, assign/update/resolve actions)
- [ ] Build the notifications UI (in-app alerts for status changes)
- [ ] Add client-side validation and loading/error states

**Elikplim Yevu**
- [ ] Build the student complaint submission form (with file attachment upload)
- [ ] Build the student complaint tracking page (list + live status)
- [ ] Build the management reporting dashboard (charts from the analytics API)
- [ ] Wire all pages to backend API endpoints

## UI/UX Designer

**Roselyn Francis**
- [x] Map user flows for students (submit → track → receive updates)
- [x] Wireframe the admin dashboard (complaint queue, management view)
- [x] Design responsive layouts (mobile + desktop breakpoints)
- [x] Run usability check with sample students/admins, revise flows

**Quartey Obed Nii Kpakpa**
- [ ] Map user flows for admins (review → assign → resolve)
- [ ] Wireframe the student dashboard (submission form, tracking page)
- [ ] Wireframe the management reporting dashboard
- [ ] Build the design system (colors, typography, buttons, status badges), store exports in `docs/wireframes/`

## Database Engineer

**Jessica Zunuo Puozaa**
- [ ] Design the core schema: Students, Admins, Departments, Complaints
- [ ] Set up EF Core models and DbContext in `SCMS.Infrastructure`
- [ ] Write seed data for testing

**Seglah Emmanuel**
- [ ] Design the Attachments, StatusHistory, and Notifications tables
- [ ] Add indexing for common queries (complaints by student, status, department)
- [ ] Add the ER diagram to `docs/er-diagram.png`

**Collins Edumadze Egyir**
- [ ] Define relationships between Complaints, Students, and Departments
- [ ] Manage migrations as the schema evolves
- [ ] Optimize queries needed for the reporting/analytics feature

## DevOps Engineer

**Keren Asabea Acquaah**
- [ ] Manage environment configuration (connection strings, secrets, dev/staging/prod settings)
- [ ] Monitor open PRs and help resolve merge conflicts between teammates

**Rushdan Delimwine Antiku**
- [x] Set up the SQL Server environment and deployment
- [x] Set up hosting/deployment for the API and Blazor app, keep deployment/runbook notes updated
