using System.ComponentModel.DataAnnotations;

namespace SCMS.Web.Models;

/// <summary>
/// Form state for the "Submit New Complaint" page.
/// Subject/Description limits mirror ComplaintCreateDto in SCMS.API (Title 3-200 chars, Description min 10).
/// </summary>
public class ComplaintFormModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a department.")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Enter a subject.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Subject must be between 3 and 200 characters.")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Describe what happened.")]
    [MinLength(10, ErrorMessage = "Description must be at least 10 characters.")]
    public string Description { get; set; } = string.Empty;
}
