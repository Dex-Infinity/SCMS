using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>
/// Data for the management analytics screen. Implementations should throw on failure.
/// The API currently exposes GET api/reports/summary | by-status | by-department | resolution-time,
/// which don't yet cover date ranges, categories, monthly trends, per-department averages or export.
/// </summary>
public interface IAnalyticsService
{
    Task<ManagementAnalyticsData> GetAsync(ReportRange range, CancellationToken cancellationToken = default);

    Task<ExportedReport> ExportAsync(ReportRange range, ReportFormat format, CancellationToken cancellationToken = default);
}
