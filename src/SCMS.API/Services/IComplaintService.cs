using SCMS.API.DTOs;
using SCMS.Domain.Enums;

namespace SCMS.API.Services;

public interface IComplaintService
{
    Task<IEnumerable<ComplaintResponseDto>> GetAllComplaintsAsync();
    Task<ComplaintResponseDto?> GetComplaintByIdAsync(int id);
    Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByStudentIdAsync(string studentId);
    Task<ComplaintResponseDto> CreateComplaintAsync(ComplaintCreateDto createDto);
    Task<ComplaintResponseDto?> UpdateStatusAsync(int id, ComplaintStatus status);
    Task<ComplaintResponseDto?> AssignComplaintAsync(int id, string departmentId, string? assignedTo);
}
