using System.ComponentModel.DataAnnotations;

namespace SCMS.Web.Models;

public sealed class ApiComplaint
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public ComplaintStatus Status { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class ApiNotification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? ComplaintId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ApiAttachment
{
    public int Id { get; set; }
    public int ComplaintId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public sealed class ApiNotificationCreate
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? ComplaintId { get; set; }
}

public sealed class ProfileFormModel
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string UserName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string Email { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
}

public sealed class SettingsFormModel
{
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; }

    [Required, RegularExpression("^(light|dark|system)$")]
    public string Theme { get; set; } = "light";

    [Required, StringLength(10, MinimumLength = 2)]
    public string Language { get; set; } = "en";
}

public sealed class ApiProfile
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = [];
}

public sealed class ApiUserSettings
{
    public string UserId { get; set; } = string.Empty;
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en";
}

public sealed class ComplaintStatusUpdate
{
    public ComplaintStatus Status { get; set; }
}

public sealed class ComplaintAssignment
{
    [Range(1, int.MaxValue, ErrorMessage = "Choose a valid department ID.")]
    public int DepartmentId { get; set; }
    public int? AssignedToId { get; set; }
}

public sealed class SettingsUpdate
{
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en";
}

public sealed class ProfileUpdate
{
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public sealed class DepartmentOption
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}