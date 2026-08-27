using SCMS.Domain.Entities;
using SCMS.Domain.Enums;

namespace SCMS.Infrastructure.Data;

/// <summary>
/// Seeds the database with test data for development/demo purposes.
/// Call SeedAsync(context) once during startup (e.g. in Program.cs, behind an
/// "if (app.Environment.IsDevelopment())" check) after migrations have run.
///
/// UserId fields are left null here since Identity isn't wired up in this repo yet -
/// once Amartey's auth work lands, real Identity user IDs can be linked in.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Departments.Any())
            return; // already seeded

        // ---- Departments ----
        var csDept = new Department { Name = "Department of Computer Science", Code = "DCSC" };
        var lawDept = new Department { Name = "University of Ghana School of Law", Code = "LAW" };
        var busDept = new Department { Name = "University of Ghana Business School", Code = "UGBS" };
        var engDept = new Department { Name = "School of Engineering Sciences", Code = "SES" };

        context.Departments.AddRange(csDept, lawDept, busDept, engDept);
        await context.SaveChangesAsync();

        // ---- Admins ----
        var superAdmin = new Admin
        {
            FullName = "Nana Kofi Agyin",
            Email = "n.agyin@st.ug.edu.gh",
            Role = AdminRole.SuperAdmin,
            DepartmentId = null
        };

        var csAdmin = new Admin
        {
            FullName = "Keren Asabea Acquaah",
            Email = "k.acquaah@st.ug.edu.gh",
            Role = AdminRole.DepartmentAdmin,
            DepartmentId = csDept.Id
        };

        context.Admins.AddRange(superAdmin, csAdmin);
        await context.SaveChangesAsync();

        // ---- Students ----
        var student1 = new Student
        {
            IndexNumber = "10912345",
            FullName = "Jessica Zunuo Puozaa",
            Email = "jz.puozaa@st.ug.edu.gh",
            PhoneNumber = "0244000001",
            EnrollmentYear = 2023,
            DepartmentId = csDept.Id
        };

        var student2 = new Student
        {
            IndexNumber = "10912678",
            FullName = "Elikplim Yevu",
            Email = "e.yevu@st.ug.edu.gh",
            PhoneNumber = "0244000002",
            EnrollmentYear = 2022,
            DepartmentId = busDept.Id
        };

        var student3 = new Student
        {
            IndexNumber = "10911987",
            FullName = "Collins Edumadze Egyir",
            Email = "c.egyir@st.ug.edu.gh",
            PhoneNumber = "0244000003",
            EnrollmentYear = 2023,
            DepartmentId = engDept.Id
        };

        context.Students.AddRange(student1, student2, student3);
        await context.SaveChangesAsync();

        // ---- Complaints (varied statuses, to exercise tracking/reporting later) ----
        var complaint1 = new Complaint
        {
            Title = "Delayed grade correction",
            Description = "Requested a grade correction for DCIT 208 three weeks ago; no update yet.",
            Status = ComplaintStatus.Assigned,
            StudentId = student1.Id,
            DepartmentId = csDept.Id,
            AssignedAdminId = csAdmin.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var complaint2 = new Complaint
        {
            Title = "Hostel room maintenance not addressed",
            Description = "Reported a broken window lock at Volta Hall over a week ago.",
            Status = ComplaintStatus.Pending,
            StudentId = student2.Id,
            DepartmentId = busDept.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var complaint3 = new Complaint
        {
            Title = "Incorrect fees billed on student portal",
            Description = "Portal shows an extra hostel fee that was already paid in person.",
            Status = ComplaintStatus.Resolved,
            StudentId = student3.Id,
            DepartmentId = engDept.Id,
            AssignedAdminId = superAdmin.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-15)
        };

        context.Complaints.AddRange(complaint1, complaint2, complaint3);
        await context.SaveChangesAsync();
    }
}
