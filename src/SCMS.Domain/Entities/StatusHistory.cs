using SCMS.Domain.Enums;

namespace SCMS.Domain.Entities;

// Tracks every status transition a complaint goes through for audit and reporting
public class StatusHistory
{
    public int Id { get; set; }

    // The complaint this history entry belongs to
    public int ComplaintId { get; set; }

    // Status before the transition
    public ComplaintStatus FromStatus { get; set; }

    // Status after the transition
    public ComplaintStatus ToStatus { get; set; }

    // ASP.NET Identity UserId of the person who made the change
    public string ChangedByUserId { get; set; } = string.Empty;

    // Optional note explaining the reason for the change
    public string? Note { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Complaint? Complaint { get; set; }
}
