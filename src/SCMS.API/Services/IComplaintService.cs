using SCMS.API.DTOs;
using SCMS.Domain.Enums;

namespace SCMS.API.Services;

public interface IComplaintService
{
    Task<IEnumerable<ComplaintResponseDto>> GetAllComplaintsAsync();
    Task<ComplaintResponseDto?> GetComplaintByIdAsync(int id);
    Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByStudentIdAsync(int studentId);
    Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByDepartmentIdAsync(int departmentId);
    Task<ComplaintResponseDto> CreateComplaintAsync(ComplaintCreateDto createDto);
    Task<ComplaintResponseDto?> UpdateStatusAsync(int id, ComplaintStatus status, string? comment = null, string? changedBy = null);
    Task<ComplaintResponseDto?> AssignComplaintAsync(int id, int departmentId, int? assignedToId);
    Task<IEnumerable<StatusHistoryResponseDto>> GetStatusHistoryAsync(int complaintId);
}
