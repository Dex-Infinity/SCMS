using System.Globalization;
using System.Text;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>Sample data from the design. Replace with an API-backed implementation.</summary>
public sealed class MockAnalyticsService : IAnalyticsService
{
    public Task<ManagementAnalyticsData> GetAsync(ReportRange range, CancellationToken cancellationToken = default)
    {
        // Scale the counts per range just so changing the range visibly does something.
        var scale = range switch
        {
            ReportRange.CurrentSemester => 3,
            ReportRange.YearToDate => 8,
            _ => 1
        };

        var data = new ManagementAnalyticsData(
            ComplaintsByCategory:
            [
                new("Academic", 145 * scale),
                new("Facilities", 98 * scale),
                new("Housing", 64 * scale),
                new("Financial", 42 * scale),
                new("Other", 21 * scale)
            ],
            ResolutionRate: 87,
            ResolutionRateChange: 2.4,
            ResolutionTrend:
            [
                new("Jan", 74),
                new("Feb", 73),
                new("Mar", 83),
                new("Apr", 87)
            ],
            Departments:
            [
                new("Student Life", 2.4, DepartmentRating.Excellent),
                new("IT Services", 3.1, DepartmentRating.Excellent),
                new("Academic Advising", 4.5, DepartmentRating.Average),
                new("Campus Housing", 6.2, DepartmentRating.NeedsAttention)
            ]);

        return Task.FromResult(data);
    }

    public async Task<ExportedReport> ExportAsync(ReportRange range, ReportFormat format, CancellationToken cancellationToken = default)
    {
        if (format == ReportFormat.Pdf)
        {
            throw new NotSupportedException("PDF export needs the reporting API, which isn't available yet.");
        }

        var data = await GetAsync(range, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine($"SCMS management report,{Escape(ReportRanges.LabelFor(range))}");
        csv.AppendLine();
        csv.AppendLine("Category,Complaints");
        foreach (var category in data.ComplaintsByCategory)
        {
            csv.AppendLine(string.Create(CultureInfo.InvariantCulture, $"{Escape(category.Label)},{category.Value:0}"));
        }

        csv.AppendLine();
        csv.AppendLine("Department,Average resolution (days),Rating");
        foreach (var department in data.Departments)
        {
            csv.AppendLine(string.Create(CultureInfo.InvariantCulture, $"{Escape(department.Name)},{department.AverageDays:0.0},{department.Rating}"));
        }

        // UTF-8 with a byte-order mark so Excel opens it correctly.
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var bytes = encoding.GetPreamble().Concat(encoding.GetBytes(csv.ToString())).ToArray();

        var fileName = $"scms-report-{range.ToString().ToLowerInvariant()}-{DateTime.Today:yyyyMMdd}.csv";
        return new ExportedReport(fileName, "text/csv", bytes);
    }

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
