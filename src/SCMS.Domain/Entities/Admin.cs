using SCMS.Domain.Enums;

namespace SCMS.Domain.Entities;

public class Admin
{
    public int Id { get; set; }

    // Placeholder link to whatever ASP.NET Identity user record Amartey's auth work creates.
    public string? UserId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public AdminRole Role { get; set; } = AdminRole.DepartmentAdmin;

    // Nullable: a SuperAdmin oversees all departments and has no single DepartmentId
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Complaint> AssignedComplaints { get; set; } = new List<Complaint>();
}
