namespace SCMS.Domain.Entities;

public class UserSetting
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
