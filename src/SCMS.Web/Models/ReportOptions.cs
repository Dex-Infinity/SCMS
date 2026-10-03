namespace SCMS.Web.Models;

public enum ReportRange
{
    Last30Days,
    CurrentSemester,
    YearToDate
}

public enum ReportFormat
{
    Pdf,
    Csv
}

public static class ReportRanges
{
    public static string LabelFor(ReportRange range) => range switch
    {
        ReportRange.Last30Days => "Last 30 Days",
        ReportRange.CurrentSemester => "Current Semester",
        ReportRange.YearToDate => "Year to Date",
        _ => range.ToString()
    };
}
