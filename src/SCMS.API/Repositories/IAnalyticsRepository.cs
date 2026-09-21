using SCMS.API.DTOs;

namespace SCMS.API.Repositories;

public interface IAnalyticsRepository
{
    Task<List<StatusCountDto>> CountByStatusAsync();
    Task<List<DepartmentCountDto>> CountByDepartmentAsync();
    Task<ResolutionTimeDto> GetResolutionTimeAsync();
}