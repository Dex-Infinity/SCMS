using SCMS.Domain.Entities;

namespace SCMS.API.Services;

public interface INotificationService
{
    Task NotifyStatusChangeAsync(int complaintId, int studentId, string newStatus);
    Task NotifyAssignmentAsync(int complaintId, int departmentId, int? assignedToId);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
}
