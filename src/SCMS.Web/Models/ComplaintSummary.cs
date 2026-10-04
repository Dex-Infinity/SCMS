namespace SCMS.Web.Models;

/// <summary>One row in a complaints list (dashboard, "My Complaints").</summary>
public sealed record ComplaintSummary(
    int Id,
    string Reference,
    string Subject,
    string Category,
    DateTime SubmittedAt,
    ComplaintStatus Status,
    string Description = "");
