using SCMS.Domain.Entities;
using SCMS.Domain.Enums;

namespace SCMS.API.Repositories;

public interface IComplaintRepository
{
    Task<IEnumerable<Complaint>> GetAllAsync();
    Task<Complaint?> GetByIdAsync(int id);
    Task<IEnumerable<Complaint>> GetByStudentIdAsync(string studentId);
    Task<IEnumerable<Complaint>> GetByDepartmentIdAsync(string departmentId);
    Task<Complaint> CreateAsync(Complaint complaint);
    Task<Complaint?> UpdateStatusAsync(int id, ComplaintStatus status);
    Task<Complaint?> AssignAsync(int id, string departmentId, string? assignedTo);
}
