using SCMS.API.DTOs;
using SCMS.API.Repositories;
using SCMS.Domain.Enums;

namespace SCMS.API.Services;

public interface IAnalyticsService
{
    Task<ReportsSummaryDto> GetSummaryAsync();
    Task<List<StatusCountDto>> CountByStatusAsync();
    Task<List<DepartmentCountDto>> CountByDepartmentAsync();
    Task<ResolutionTimeDto> GetResolutionTimeAsync();
}

// Service for building reporting and analytics payloads
public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly IComplaintRepository _complaintRepository;

    // Inject analytics and complaint repositories
    public AnalyticsService(IAnalyticsRepository analyticsRepository, IComplaintRepository complaintRepository)
    {
        _analyticsRepository = analyticsRepository;
        _complaintRepository = complaintRepository;
    }

    // Build a full report summary (totals, counts by status/department, resolution time)
    public async Task<ReportsSummaryDto> GetSummaryAsync()
    {
        var complaints = (await _complaintRepository.GetAllAsync()).ToList();

        var byStatus = await _analyticsRepository.CountByStatusAsync();
        var byDepartment = await _analyticsRepository.CountByDepartmentAsync();
        var resolutionTime = await _analyticsRepository.GetResolutionTimeAsync();

        return new ReportsSummaryDto
        {
            Total = complaints.Count,
            Pending = complaints.Count(c => c.Status == ComplaintStatus.Pending),
            UnderReview = complaints.Count(c => c.Status == ComplaintStatus.UnderReview),
            Assigned = complaints.Count(c => c.Status == ComplaintStatus.Assigned),
            Resolved = complaints.Count(c => c.Status == ComplaintStatus.Resolved),
            Rejected = complaints.Count(c => c.Status == ComplaintStatus.Rejected),
            ByStatus = byStatus,
            ByDepartment = byDepartment,
            ResolutionTime = resolutionTime
        };
    }

    public async Task<List<StatusCountDto>> CountByStatusAsync()
    {
        return await _analyticsRepository.CountByStatusAsync();
    }

    public async Task<List<DepartmentCountDto>> CountByDepartmentAsync()
    {
        return await _analyticsRepository.CountByDepartmentAsync();
    }

    public async Task<ResolutionTimeDto> GetResolutionTimeAsync()
    {
        return await _analyticsRepository.GetResolutionTimeAsync();
    }
}