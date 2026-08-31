namespace SCMS.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Admin> Admins { get; set; } = new List<Admin>();
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
