namespace SCMS.Web.Models;

public static class ComplaintStatusExtensions
{
    public static string ToLabel(this ComplaintStatus status) => status switch
    {
        ComplaintStatus.Pending => "Pending",
        ComplaintStatus.UnderReview => "Under Review",
        ComplaintStatus.Assigned => "Assigned",
        ComplaintStatus.Resolved => "Resolved",
        ComplaintStatus.Rejected => "Rejected",
        _ => status.ToString()
    };
}
