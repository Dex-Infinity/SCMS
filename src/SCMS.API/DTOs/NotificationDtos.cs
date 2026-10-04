using System.ComponentModel.DataAnnotations;

namespace SCMS.API.DTOs;

// Data transfer object representing a notification returned to client
public class NotificationResponseDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? ComplaintId { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Data transfer object for creating a notification
public class NotificationCreateDto
{
    [Required(ErrorMessage = "Target user ID is required.")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Notification title is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Notification message is required.")]
    public string Message { get; set; } = string.Empty;

    public int? ComplaintId { get; set; }
}

// Data transfer object for unread count badge
public class NotificationUnreadCountDto
{
    public int UnreadCount { get; set; }
}
