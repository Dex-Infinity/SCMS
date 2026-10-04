namespace SCMS.Web.Models;

public sealed record ManagementAnalyticsData(
    int Total,
    int Pending,
    int UnderReview,
    int Assigned,
    int Resolved,
    int Rejected,
    double ResolutionRate,
    double AverageResolutionHours,
    double MedianResolutionHours,
    IReadOnlyList<ChartPoint> ByStatus,
    IReadOnlyList<ChartPoint> ByDepartment);

public sealed record ExportedReport(string FileName, string ContentType, byte[] Content);
