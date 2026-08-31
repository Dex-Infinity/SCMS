namespace SCMS.Domain.Entities;

public class Student
{
    public int Id { get; set; }
    public string IndexNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Department? Department { get; set; }
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
