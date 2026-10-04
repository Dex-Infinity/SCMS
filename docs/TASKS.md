# SCMS Task Assignments

Reference list of tasks per team member. For live tracking, mirror these as GitHub Issues on the Projects board (see below).

## Backend Developer

**Virtus Dakura**
- [x] Set up the ASP.NET Core Web API project structure (Controllers, Services, Repositories, DTOs)
- [x] Build Complaint endpoints — create, get by student, get by ID, update status, assign to department
- [x] Build Notification service — triggers updates on status change
- [x] Write Swagger/OpenAPI docs for the API
- [x] Build NotificationsController to expose user notifications
- [x] Update ComplaintsController to allow students to fetch their own complaint details by ID

**Amartey Felix Laryea**
- [x] Implement ASP.NET Identity — registration, login, student/admin roles, JWT/cookie auth
- [x] Build Attachment handling — upload/download supporting documents
- [x] Build Reports/Analytics endpoints — counts by status, department, resolution time
- [x] Add validation and centralized error handling
- [x] Build Profile API endpoint for students to update personal details
- [x] Build Settings API endpoint to save user preferences

## Frontend Developer

**Irene Darah-Mensah**
- [x] Set up the Blazor project and shared layout (nav, auth-aware routing)
- [x] Build the admin dashboard (queue, filters, assign/update/resolve actions)
- [x] Build the notifications UI (in-app alerts for status changes)
- [x] Add client-side validation and loading/error states
- [x] Build the User Profile page (`/profile`)
- [x] Build the Settings page (`/settings`)

**Elikplim Yevu**
- [x] Build the student complaint submission form (with file attachment upload)
- [x] Build the student complaint tracking page (list + live status)
- [x] Build the management reporting dashboard (charts from the analytics API)
- [ ] Wire all pages to backend API endpoints
- [ ] Build the "All Complaints" list page (`/complaints`) and details page (`/complaints/{id}`)

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
- [x] Build the design system (colors, typography, buttons, status badges), store exports in `docs/wireframes/`

## Database Engineer

**Jessica Zunuo Puozaa**
- [x] Design the core schema: Students, Admins, Departments, Complaints
- [x] Set up EF Core models and DbContext in `SCMS.Infrastructure`
- [x] Write seed data for testing

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
