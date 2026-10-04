using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>
/// Data for the management analytics screen. Implementations should throw on failure.
/// The API provides summary, status, department, and resolution-time report endpoints.
/// </summary>
public interface IAnalyticsService
{
    Task<ManagementAnalyticsData> GetAsync(CancellationToken cancellationToken = default);

    Task<ExportedReport> ExportAsync(CancellationToken cancellationToken = default);
}
