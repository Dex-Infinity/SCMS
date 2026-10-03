namespace SCMS.Web.Models;

public enum DepartmentRating
{
    Excellent,
    Average,
    NeedsAttention
}

public sealed record DepartmentEfficiency(string Name, double AverageDays, DepartmentRating Rating);

public sealed record ManagementAnalyticsData(
    IReadOnlyList<ChartPoint> ComplaintsByCategory,
    double ResolutionRate,
    double ResolutionRateChange,
    IReadOnlyList<ChartPoint> ResolutionTrend,
    IReadOnlyList<DepartmentEfficiency> Departments);

public sealed record ExportedReport(string FileName, string ContentType, byte[] Content);
