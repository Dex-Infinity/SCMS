namespace SCMS.Domain.Entities;

public class Student
{
    public int Id { get; set; }

    // Placeholder link to whatever ASP.NET Identity user record Amartey's auth work creates.
    public string? UserId { get; set; }

    public string IndexNumber { get; set; } = string.Empty;   // e.g. "10912345"
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int EnrollmentYear { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
