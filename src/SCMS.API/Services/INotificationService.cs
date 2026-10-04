using SCMS.API.DTOs;
using SCMS.Domain.Entities;

namespace SCMS.API.Services;

public interface INotificationService
{
    // Automated event notifications
    Task NotifyStatusChangeAsync(int complaintId, int studentId, string newStatus);
    Task NotifyAssignmentAsync(int complaintId, int departmentId, int? assignedToId);

    // Entity-based notifications (backward compatibility)
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);

    // DTO-based notifications for frontend API
    Task<IEnumerable<NotificationResponseDto>> GetNotificationsForUserAsync(string userId, int? studentId = null, bool? unreadOnly = null);
    Task<NotificationResponseDto?> GetNotificationByIdAsync(int id);
    Task<int> GetUnreadCountAsync(string userId, int? studentId = null);
    Task<NotificationResponseDto?> MarkAsReadAsync(int id, string? userId = null, int? studentId = null);
    Task<int> MarkAllAsReadAsync(string userId, int? studentId = null);
    Task<NotificationResponseDto> CreateNotificationAsync(NotificationCreateDto dto);
    Task<bool> DeleteNotificationAsync(int id, string? userId = null, int? studentId = null);
}
