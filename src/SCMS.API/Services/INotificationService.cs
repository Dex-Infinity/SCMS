using SCMS.Domain.Entities;

namespace SCMS.API.Services;

public interface INotificationService
{
    Task NotifyStatusChangeAsync(int complaintId, string studentId, string newStatus);
    Task NotifyAssignmentAsync(int complaintId, string departmentId, string? assignedTo);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
}
