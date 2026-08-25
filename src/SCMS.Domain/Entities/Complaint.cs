using SCMS.Domain.Enums;

namespace SCMS.Domain.Entities;

public class Complaint
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
