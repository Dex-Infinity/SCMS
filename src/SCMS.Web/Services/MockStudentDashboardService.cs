using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>Sample data from the design so the page can be built before the API is wired up.</summary>
public sealed class MockStudentDashboardService : IStudentDashboardService
{
    public Task<StudentDashboardData> GetAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ComplaintSummary> recent =
        [
            new(89, "CMP-2023-089", "Heating issue in Dorm B", "Facilities", new DateTime(2023, 10, 24), ComplaintStatus.Pending),
            new(85, "CMP-2023-085", "Grade dispute for CS101", "Academic", new DateTime(2023, 10, 18), ComplaintStatus.UnderReview),
            new(72, "CMP-2023-072", "Broken projector in Room 402", "IT Support", new DateTime(2023, 9, 5), ComplaintStatus.Resolved)
        ];

        return Task.FromResult(new StudentDashboardData(TotalFiled: 12, PendingReview: 3, Resolved: 9, Recent: recent));
    }
}
