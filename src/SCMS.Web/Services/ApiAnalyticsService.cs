using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiAnalyticsService(ApiClient apiClient) : IAnalyticsService
{
    public async Task<ManagementAnalyticsData> GetAsync(CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(cancellationToken);
        var resolutionRate = summary.Total == 0 ? 0 : summary.Resolved * 100d / summary.Total;

        return new ManagementAnalyticsData(
            summary.Total,
            summary.Pending,
            summary.UnderReview,
            summary.Assigned,
            summary.Resolved,
            summary.Rejected,
            resolutionRate,
            summary.ResolutionTime.AverageHours,
            summary.ResolutionTime.MedianHours,
            summary.ByStatus.Select(item => new ChartPoint(item.Status, item.Count)).ToList(),
            summary.ByDepartment.Select(item => new ChartPoint(
                string.IsNullOrWhiteSpace(item.DepartmentId) ? "Unassigned" : $"Department {item.DepartmentId}", item.Count)).ToList());
    }

    public async Task<ExportedReport> ExportAsync(CancellationToken cancellationToken = default)
    {
        var data = await GetAsync(cancellationToken);
        var csv = new StringBuilder();
        csv.AppendLine("SCMS all-time management report");
        csv.AppendLine();
        csv.AppendLine("Metric,Value");
        csv.AppendLine(FormattableString.Invariant($"Total complaints,{data.Total}"));
        csv.AppendLine(FormattableString.Invariant($"Resolved complaints,{data.Resolved}"));
        csv.AppendLine(FormattableString.Invariant($"Resolution rate,{data.ResolutionRate:0.0}%"));
        csv.AppendLine(FormattableString.Invariant($"Average resolution hours,{data.AverageResolutionHours:0.0}"));
        csv.AppendLine(FormattableString.Invariant($"Median resolution hours,{data.MedianResolutionHours:0.0}"));
        csv.AppendLine();
        csv.AppendLine("Status,Complaints");
        foreach (var item in data.ByStatus)
        {
            csv.AppendLine($"{Escape(item.Label)},{item.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        csv.AppendLine();
        csv.AppendLine("Department,Complaints");
        foreach (var item in data.ByDepartment)
        {
            csv.AppendLine($"{Escape(item.Label)},{item.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var bytes = encoding.GetPreamble().Concat(encoding.GetBytes(csv.ToString())).ToArray();
        return new ExportedReport($"scms-report-{DateTime.UtcNow:yyyyMMdd}.csv", "text/csv", bytes);
    }

    private async Task<ApiReportsSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/reports/summary");
        using var response = await apiClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiReportsSummary>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty reports summary.");
    }

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}