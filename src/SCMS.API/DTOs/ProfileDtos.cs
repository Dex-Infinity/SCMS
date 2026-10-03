using System.ComponentModel.DataAnnotations;

namespace SCMS.API.DTOs;

// Data transfer object returned when fetching the current user's profile
public class ProfileResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

// Data transfer object for students updating their personal details
public class ProfileUpdateDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
    public string UserName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Phone number is not valid.")]
    public string? PhoneNumber { get; set; }
}
