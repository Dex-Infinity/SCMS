using SCMS.API.DTOs;
using SCMS.API.Repositories;
using SCMS.Domain.Entities;
using SCMS.Domain.Enums;

namespace SCMS.API.Services;

// Service for complaint business logic and notification dispatching
public class ComplaintService : IComplaintService
{
    private readonly IComplaintRepository _repository;
    private readonly INotificationService _notificationService;

    public ComplaintService(IComplaintRepository repository, INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }

    // Fetch all complaints
    public async Task<IEnumerable<ComplaintResponseDto>> GetAllComplaintsAsync()
    {
        var complaints = await _repository.GetAllAsync();
        return complaints.Select(MapToResponseDto);
    }

    // Fetch single complaint by ID
    public async Task<ComplaintResponseDto?> GetComplaintByIdAsync(int id)
    {
        var complaint = await _repository.GetByIdAsync(id);
        return complaint == null ? null : MapToResponseDto(complaint);
    }

    // Fetch all complaints for a specific student ID
    public async Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByStudentIdAsync(int studentId)
    {
        var complaints = await _repository.GetByStudentIdAsync(studentId);
        return complaints.Select(MapToResponseDto);
    }

    // Fetch all complaints assigned to a department ID
    public async Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByDepartmentIdAsync(int departmentId)
    {
        var complaints = await _repository.GetByDepartmentIdAsync(departmentId);
        return complaints.Select(MapToResponseDto);
    }

    // Create a new complaint record
    public async Task<ComplaintResponseDto> CreateComplaintAsync(ComplaintCreateDto createDto)
    {
        var complaint = new Complaint
        {
            Title = createDto.Title,
            Description = createDto.Description,
            StudentId = createDto.StudentId,
            DepartmentId = createDto.DepartmentId,
            Status = ComplaintStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(complaint);
        return MapToResponseDto(created);
    }

    // Update status and send alert notification
    public async Task<ComplaintResponseDto?> UpdateStatusAsync(int id, ComplaintStatus status)
    {
        var updated = await _repository.UpdateStatusAsync(id, status);
        if (updated != null)
        {
            await _notificationService.NotifyStatusChangeAsync(updated.Id, updated.StudentId, status.ToString());
            return MapToResponseDto(updated);
        }

        return null;
    }

    // Assign complaint to department/staff and send alert notification
    public async Task<ComplaintResponseDto?> AssignComplaintAsync(int id, int departmentId, int? assignedToId)
    {
        var assigned = await _repository.AssignAsync(id, departmentId, assignedToId);
        if (assigned != null)
        {
            await _notificationService.NotifyAssignmentAsync(assigned.Id, departmentId, assignedToId);
            return MapToResponseDto(assigned);
        }

        return null;
    }

    // Helper: Map Complaint entity to ComplaintResponseDto with enriched entity names
    private static ComplaintResponseDto MapToResponseDto(Complaint complaint)
    {
        return new ComplaintResponseDto
        {
            Id = complaint.Id,
            Title = complaint.Title,
            Description = complaint.Description,
            StudentId = complaint.StudentId,
            StudentName = complaint.Student?.FullName,
            DepartmentId = complaint.DepartmentId,
            DepartmentName = complaint.Department?.Name,
            Status = complaint.Status,
            AssignedToId = complaint.AssignedToId,
            AssignedToName = complaint.AssignedTo?.FullName,
            CreatedAt = complaint.CreatedAt,
            UpdatedAt = complaint.UpdatedAt
        };
    }
}
