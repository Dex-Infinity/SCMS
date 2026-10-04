using SCMS.API.DTOs;
using SCMS.API.Repositories;
using SCMS.Domain.Entities;
using SCMS.Domain.Enums;

namespace SCMS.API.Services;

// Service for complaint business logic, audit history, and notification dispatching
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

    // Create a new complaint record with initial status history audit
    public async Task<ComplaintResponseDto> CreateComplaintAsync(ComplaintCreateDto createDto)
    {
        var complaint = new Complaint
        {
            Title = createDto.Title,
            Description = createDto.Description,
            StudentId = createDto.StudentId,
            DepartmentId = createDto.DepartmentId,
            Priority = createDto.Priority,
            Status = ComplaintStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(complaint);

        // Record initial status history entry
        await _repository.AddStatusHistoryAsync(new StatusHistory
        {
            ComplaintId = created.Id,
            Status = ComplaintStatus.Pending,
            Comment = "Complaint submitted.",
            ChangedBy = created.StudentName ?? $"Student #{created.StudentId}",
            ChangedAt = DateTime.UtcNow
        });

        return MapToResponseDto(created);
    }

    // Update status, persist audit history, and send alert notification
    public async Task<ComplaintResponseDto?> UpdateStatusAsync(int id, ComplaintStatus status, string? comment = null, string? changedBy = null)
    {
        var updated = await _repository.UpdateStatusAsync(id, status);
        if (updated != null)
        {
            await _repository.AddStatusHistoryAsync(new StatusHistory
            {
                ComplaintId = updated.Id,
                Status = status,
                Comment = !string.IsNullOrWhiteSpace(comment) ? comment : $"Status updated to '{status}'.",
                ChangedBy = !string.IsNullOrWhiteSpace(changedBy) ? changedBy : "Admin",
                ChangedAt = DateTime.UtcNow
            });

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
            await _repository.AddStatusHistoryAsync(new StatusHistory
            {
                ComplaintId = assigned.Id,
                Status = ComplaintStatus.Assigned,
                Comment = assignedToId.HasValue
                    ? $"Assigned to department #{departmentId} and staff member #{assignedToId.Value}."
                    : $"Assigned to department #{departmentId}.",
                ChangedBy = "Admin",
                ChangedAt = DateTime.UtcNow
            });

            await _notificationService.NotifyAssignmentAsync(assigned.Id, departmentId, assignedToId);
            return MapToResponseDto(assigned);
        }

        return null;
    }

    // Fetch chronological status audit history for a complaint
    public async Task<IEnumerable<StatusHistoryResponseDto>> GetStatusHistoryAsync(int complaintId)
    {
        var histories = await _repository.GetStatusHistoryAsync(complaintId);
        return histories.Select(h => new StatusHistoryResponseDto
        {
            Id = h.Id,
            ComplaintId = h.ComplaintId,
            Status = h.Status,
            Comment = h.Comment,
            ChangedBy = h.ChangedBy,
            ChangedAt = h.ChangedAt
        });
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
            StudentEmail = complaint.Student?.Email,
            DepartmentId = complaint.DepartmentId,
            DepartmentName = complaint.Department?.Name,
            Status = complaint.Status,
            Priority = complaint.Priority,
            AssignedToId = complaint.AssignedToId,
            AssignedToName = complaint.AssignedTo?.FullName,
            CreatedAt = complaint.CreatedAt,
            UpdatedAt = complaint.UpdatedAt
        };
    }
}
