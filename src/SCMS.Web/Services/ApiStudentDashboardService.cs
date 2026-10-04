using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiStudentDashboardService(IMyComplaintsService complaints) : IStudentDashboardService
{
    public async Task<StudentDashboardData> GetAsync(CancellationToken cancellationToken = default)
    {
        var items = await complaints.GetAsync(cancellationToken);
        var activeCount = items.Count(complaint => complaint.Status is ComplaintStatus.Pending or ComplaintStatus.UnderReview or ComplaintStatus.Assigned);
        var recent = items.OrderByDescending(complaint => complaint.SubmittedAt).Take(3).ToList();

        return new StudentDashboardData(items.Count, activeCount,
            items.Count(complaint => complaint.Status == ComplaintStatus.Resolved), recent);
    }
}