namespace SCMS.API.DTOs;

// Data transfer object for a count grouped by complaint status
public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

// Data transfer object for a count grouped by department
public class DepartmentCountDto
{
    public string? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "Unassigned";
    public int Count { get; set; }
}

// Data transfer object for complaint resolution duration statistics
public class ResolutionTimeDto
{
    public int ResolvedCount { get; set; }
    public double AverageHours { get; set; }
    public double MedianHours { get; set; }
}

// Data transfer object returned by the reports summary endpoint
public class ReportsSummaryDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int UnderReview { get; set; }
    public int Assigned { get; set; }
    public int Resolved { get; set; }
    public int Rejected { get; set; }
    public List<StatusCountDto> ByStatus { get; set; } = new();
    public List<DepartmentCountDto> ByDepartment { get; set; } = new();
    public ResolutionTimeDto ResolutionTime { get; set; } = new();
}