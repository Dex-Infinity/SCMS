using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.Domain.Entities;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Services;

// Service for generating, fetching, and managing user notifications
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Send notification to student when complaint status changes
    public async Task NotifyStatusChangeAsync(int complaintId, int studentId, string newStatus)
    {
        var notification = new Notification
        {
            UserId = studentId.ToString(),
            ComplaintId = complaintId,
            Title = "Complaint Status Updated",
            Message = $"Your complaint #{complaintId} status has been updated to '{newStatus}'.",
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    // Send notification to department/staff when assigned
    public async Task NotifyAssignmentAsync(int complaintId, int departmentId, int? assignedToId)
    {
        string targetUser = assignedToId.HasValue ? $"Admin-{assignedToId.Value}" : $"Dept-{departmentId}";

        var notification = new Notification
        {
            UserId = targetUser,
            ComplaintId = complaintId,
            Title = "New Complaint Assigned",
            Message = $"Complaint #{complaintId} has been assigned to department #{departmentId}.",
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    // Get list of notifications for a specific user (entity-based)
    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    // Get list of notifications for a user, checking both Identity UserId and StudentId (DTO-based)
    public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsForUserAsync(
        string userId, int? studentId = null, bool? unreadOnly = null)
    {
        var allowedIds = GetMatchingUserIds(userId, studentId);

        var query = _context.Notifications
            .Where(n => allowedIds.Contains(n.UserId));

        if (unreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToDto);
    }

    // Fetch single notification by ID
    public async Task<NotificationResponseDto?> GetNotificationByIdAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        return notification == null ? null : MapToDto(notification);
    }

    // Get count of unread notifications for a user
    public async Task<int> GetUnreadCountAsync(string userId, int? studentId = null)
    {
        var allowedIds = GetMatchingUserIds(userId, studentId);

        return await _context.Notifications
            .CountAsync(n => allowedIds.Contains(n.UserId) && !n.IsRead);
    }

    // Mark single notification as read
    public async Task<NotificationResponseDto?> MarkAsReadAsync(int id, string? userId = null, int? studentId = null)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return null;

        if (!string.IsNullOrEmpty(userId))
        {
            var allowedIds = GetMatchingUserIds(userId, studentId);
            if (!allowedIds.Contains(notification.UserId))
            {
                return null;
            }
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return MapToDto(notification);
    }

    // Mark all notifications for a user as read
    public async Task<int> MarkAllAsReadAsync(string userId, int? studentId = null)
    {
        var allowedIds = GetMatchingUserIds(userId, studentId);

        var unread = await _context.Notifications
            .Where(n => allowedIds.Contains(n.UserId) && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return unread.Count;
    }

    // Create a new notification manually
    public async Task<NotificationResponseDto> CreateNotificationAsync(NotificationCreateDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            ComplaintId = dto.ComplaintId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return MapToDto(notification);
    }

    // Delete a notification
    public async Task<bool> DeleteNotificationAsync(int id, string? userId = null, int? studentId = null)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return false;

        if (!string.IsNullOrEmpty(userId))
        {
            var allowedIds = GetMatchingUserIds(userId, studentId);
            if (!allowedIds.Contains(notification.UserId))
            {
                return false;
            }
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return true;
    }

    // Helper: Map Notification entity to NotificationResponseDto
    private static NotificationResponseDto MapToDto(Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            ComplaintId = notification.ComplaintId,
            CreatedAt = notification.CreatedAt
        };
    }

    // Helper: Generate set of user IDs to check
    private static HashSet<string> GetMatchingUserIds(string userId, int? studentId = null)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { userId };
        if (studentId.HasValue)
        {
            ids.Add(studentId.Value.ToString());
        }
        return ids;
    }
}
