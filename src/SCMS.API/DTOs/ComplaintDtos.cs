using System.ComponentModel.DataAnnotations;
using SCMS.Domain.Enums;

namespace SCMS.API.DTOs;

// Data transfer object for submitting a new complaint
public class ComplaintCreateDto
{
    [Required(ErrorMessage = "Complaint title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Complaint description is required.")]
    [MinLength(10, ErrorMessage = "Description must be at least 10 characters long.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "StudentId is required.")]
    public string StudentId { get; set; } = string.Empty;

    public string? DepartmentId { get; set; }
}

// Data transfer object for updating status
public class ComplaintStatusUpdateDto
{
    [Required(ErrorMessage = "Status is required.")]
    public ComplaintStatus Status { get; set; }

    public string? Comment { get; set; }
}

// Data transfer object for assigning complaint to department/staff
public class ComplaintAssignDto
{
    [Required(ErrorMessage = "DepartmentId is required.")]
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
