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
    public int StudentId { get; set; }

    public int? DepartmentId { get; set; }
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
    public int DepartmentId { get; set; }

    public int? AssignedToId { get; set; }
}

// Data transfer object returned to client
public class ComplaintResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int StudentId { get; set; }
    public string? StudentName { get; set; }

    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    public ComplaintStatus Status { get; set; }
    public string StatusText => Status.ToString();

    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
