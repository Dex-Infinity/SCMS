namespace SCMS.Domain.Entities;

public class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;      // e.g. "Department of Computer Science"
    public string Code { get; set; } = string.Empty;      // e.g. "DCSC" - short unique code

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Admin> Admins { get; set; } = new List<Admin>();
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
