namespace SCMS.Web.Services;

internal sealed class ApiComplaintResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

internal sealed class ApiProfileResponse
{
    public string FullName { get; set; } = string.Empty;
}

internal sealed class ApiReportsSummary
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int UnderReview { get; set; }
    public int Assigned { get; set; }
    public int Resolved { get; set; }
    public int Rejected { get; set; }
    public List<ApiStatusCount> ByStatus { get; set; } = [];
    public List<ApiDepartmentCount> ByDepartment { get; set; } = [];
    public ApiResolutionTime ResolutionTime { get; set; } = new();
}

internal sealed class ApiStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

internal sealed class ApiDepartmentCount
{
    public string? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "Unassigned";
    public int Count { get; set; }
}

internal sealed class ApiResolutionTime
{
    public int ResolvedCount { get; set; }
    public double AverageHours { get; set; }
    public double MedianHours { get; set; }
}