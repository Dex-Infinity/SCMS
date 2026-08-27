using SCMS.Domain.Enums;

namespace SCMS.Domain.Entities;

public class Complaint
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // CHANGED from Virtus's original (was: string StudentId) - now a real FK
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    // CHANGED from Virtus's original (was: string? DepartmentId) - now a real FK
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;

    // CHANGED from Virtus's original (was: string? AssignedTo) - now a real FK
    public int? AssignedAdminId { get; set; }
    public Admin? AssignedAdmin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
