using System.Globalization;
using System.Text;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>Sample data from the design. Replace with an API-backed implementation.</summary>
public sealed class MockAnalyticsService : IAnalyticsService
{
    public Task<ManagementAnalyticsData> GetAsync(CancellationToken cancellationToken = default)
    {
        var data = new ManagementAnalyticsData(
            Total: 370,
            Pending: 26,
            UnderReview: 47,
            Assigned: 58,
            Resolved: 222,
            Rejected: 17,
            ResolutionRate: 60,
            AverageResolutionHours: 58.4,
            MedianResolutionHours: 41.2,
            ByStatus: [new("Pending", 26), new("Under review", 47), new("Assigned", 58), new("Resolved", 222), new("Rejected", 17)],
            ByDepartment: [new("Department 1", 144), new("Department 2", 121), new("Department 3", 105)]);

        return Task.FromResult(data);
    }

    public async Task<ExportedReport> ExportAsync(CancellationToken cancellationToken = default)
    {
        var data = await GetAsync(cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("SCMS all-time management report");
        csv.AppendLine();
        csv.AppendLine("Status,Complaints");
        foreach (var status in data.ByStatus)
        {
            csv.AppendLine(string.Create(CultureInfo.InvariantCulture, $"{Escape(status.Label)},{status.Value:0}"));
        }

        csv.AppendLine();
        csv.AppendLine("Department,Complaints");
        foreach (var department in data.ByDepartment)
        {
            csv.AppendLine(string.Create(CultureInfo.InvariantCulture, $"{Escape(department.Label)},{department.Value:0}"));
        }

        // UTF-8 with a byte-order mark so Excel opens it correctly.
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var bytes = encoding.GetPreamble().Concat(encoding.GetBytes(csv.ToString())).ToArray();

        var fileName = $"scms-report-{DateTime.Today:yyyyMMdd}.csv";
        return new ExportedReport(fileName, "text/csv", bytes);
    }

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
