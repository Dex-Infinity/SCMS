using SCMS.Domain.Enums;

namespace SCMS.Domain.Entities;

// Audit log of status transitions and comments for a complaint
public class StatusHistory
{
    public int Id { get; set; }
    public int ComplaintId { get; set; }
    public ComplaintStatus Status { get; set; }
    public string? Comment { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public Complaint? Complaint { get; set; }
}
