namespace SCMS.Domain.Entities;

public class Admin
{
    public int Id { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string Role { get; set; } = "DepartmentAdmin"; // e.g. SuperAdmin, DepartmentAdmin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Department? Department { get; set; }
    public ICollection<Complaint> AssignedComplaints { get; set; } = new List<Complaint>();
}
