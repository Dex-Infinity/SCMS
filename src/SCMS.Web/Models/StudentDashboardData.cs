namespace SCMS.Web.Models;

public sealed record StudentDashboardData(
    int TotalFiled,
    int PendingReview,
    int Resolved,
    IReadOnlyList<ComplaintSummary> Recent);
