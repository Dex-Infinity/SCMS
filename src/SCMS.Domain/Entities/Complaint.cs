using SCMS.Domain.Enums;

namespace SCMS.Domain.Entities;

public class Complaint
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Foreign Keys
    public int StudentId { get; set; }
    public int? DepartmentId { get; set; }
    public int? AssignedToId { get; set; }

    public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Student? Student { get; set; }
    public Department? Department { get; set; }
    public Admin? AssignedTo { get; set; }
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
}
