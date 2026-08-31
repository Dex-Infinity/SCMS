using System.ComponentModel.DataAnnotations;
using SCMS.Domain.Enums;

namespace SCMS.API.DTOs;

// Data transfer object for submitting a new complaint
public class ComplaintCreateDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string StudentId { get; set; } = string.Empty;

    public string? DepartmentId { get; set; }
}

// Data transfer object for updating status
public class ComplaintStatusUpdateDto
{
    [Required]
    public ComplaintStatus Status { get; set; }

    public string? Comment { get; set; }
}

// Data transfer object for assigning complaint to department/staff
public class ComplaintAssignDto
{
    [Required]
    public string DepartmentId { get; set; } = string.Empty;

    public string? AssignedTo { get; set; }
}

// Data transfer object returned to client
public class ComplaintResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public ComplaintStatus Status { get; set; }
    public string StatusText => Status.ToString();
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
