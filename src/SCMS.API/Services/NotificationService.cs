using Microsoft.EntityFrameworkCore;
using SCMS.Domain.Entities;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Services;

// Service for generating and fetching user notifications
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

    // Get list of notifications for a specific user
    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}
