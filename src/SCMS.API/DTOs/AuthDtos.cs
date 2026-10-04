using System.ComponentModel.DataAnnotations;

namespace SCMS.API.DTOs;

// Data transfer object for user registration
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string IndexNumber { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }
}

// Data transfer object for user login
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

// Data transfer object returned to client after successful authentication
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int? StudentId { get; set; }
    public List<string> Roles { get; set; } = new();
}
