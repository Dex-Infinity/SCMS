using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>Sample data. Totals match the mock dashboard: 12 filed, 3 in progress, 9 resolved.</summary>
public sealed class MockMyComplaintsService : IMyComplaintsService
{
    public Task<IReadOnlyList<ComplaintSummary>> GetAsync(CancellationToken cancellationToken = default)
    {
        static ComplaintSummary C(int id, string subject, string category, int y, int m, int d, ComplaintStatus status) =>
            new(id, $"CMP-2023-{id:D3}", subject, category, new DateTime(y, m, d), status);

        IReadOnlyList<ComplaintSummary> items =
        [
            C(89, "Heating issue in Dorm B", "Facilities", 2023, 10, 24, ComplaintStatus.Pending),
            C(85, "Grade dispute for CS101", "Academic", 2023, 10, 18, ComplaintStatus.UnderReview),
            C(80, "Wi-Fi outage in the library", "IT Support", 2023, 10, 2, ComplaintStatus.Assigned),
            C(72, "Broken projector in Room 402", "IT Support", 2023, 9, 5, ComplaintStatus.Resolved),
            C(68, "Tuition fee charged twice", "Financial", 2023, 8, 29, ComplaintStatus.Resolved),
            C(61, "No feedback on thesis draft", "Academic", 2023, 8, 14, ComplaintStatus.Resolved),
            C(55, "Noise complaint in Hall C", "Housing", 2023, 7, 30, ComplaintStatus.Resolved),
            C(49, "Broken lift in Science Block", "Facilities", 2023, 7, 11, ComplaintStatus.Resolved),
            C(43, "Scholarship payment delayed", "Financial", 2023, 6, 26, ComplaintStatus.Resolved),
            C(37, "Locker allocation error", "Student Life", 2023, 6, 9, ComplaintStatus.Resolved),
            C(30, "Transcript request not processed", "Academic", 2023, 5, 22, ComplaintStatus.Resolved),
            C(24, "Cafeteria hygiene concern", "Facilities", 2023, 5, 4, ComplaintStatus.Resolved)
        ];

        return Task.FromResult(items);
    }
}
