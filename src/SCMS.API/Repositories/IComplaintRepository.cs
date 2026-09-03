using SCMS.Domain.Entities;
using SCMS.Domain.Enums;

namespace SCMS.API.Repositories;

public interface IComplaintRepository
{
    Task<IEnumerable<Complaint>> GetAllAsync();
    Task<Complaint?> GetByIdAsync(int id);
    Task<IEnumerable<Complaint>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<Complaint>> GetByDepartmentIdAsync(int departmentId);
    Task<Complaint> CreateAsync(Complaint complaint);
    Task<Complaint?> UpdateStatusAsync(int id, ComplaintStatus status);
    Task<Complaint?> AssignAsync(int id, int departmentId, int? assignedToId);
}
