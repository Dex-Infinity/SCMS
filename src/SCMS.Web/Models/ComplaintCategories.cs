namespace SCMS.Web.Models;

public sealed record ComplaintCategory(string Value, string Label);

public static class ComplaintCategories
{
    public static readonly IReadOnlyList<ComplaintCategory> All =
    [
        new("academic", "Academic (Grades, Faculty, Coursework)"),
        new("facility", "Facility (Maintenance, Safety, Access)"),
        new("financial", "Financial (Billing, Aid, Fees)"),
        new("student_life", "Student Life (Housing, Conduct, Orgs)"),
        new("other", "Other")
    ];

    public static string LabelFor(string? value) =>
        All.FirstOrDefault(c => c.Value == value)?.Label ?? "-";
}
