using System.ComponentModel.DataAnnotations;

namespace SCMS.API.DTOs;

// Data transfer object returned when fetching the current user's preferences
public class SettingsResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en";
    public DateTime UpdatedAt { get; set; }
}

// Data transfer object for saving user preferences
public class SettingsUpdateDto
{
    public bool EmailNotifications { get; set; } = true;

    public bool SmsNotifications { get; set; } = false;

    [Required(ErrorMessage = "Theme is required.")]
    [RegularExpression("^(light|dark|system)$", ErrorMessage = "Theme must be 'light', 'dark', or 'system'.")]
    public string Theme { get; set; } = "light";

    [Required(ErrorMessage = "Language is required.")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Language must be between 2 and 10 characters.")]
    public string Language { get; set; } = "en";
}
