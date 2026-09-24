using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.Domain.Enums;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Repositories;

// Repository for reporting and analytics aggregations over complaints
public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    // Inject EF Core DbContext
    public AnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Count complaints grouped by their current status
    public async Task<List<StatusCountDto>> CountByStatusAsync()
    {
        return await _context.Complaints
            .GroupBy(c => c.Status)
            .Select(g => new StatusCountDto
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderBy(s => s.Status)
            .ToListAsync();
    }

    // Count complaints grouped by the department they are assigned to
    public async Task<List<DepartmentCountDto>> CountByDepartmentAsync()
    {
        return await _context.Complaints
            .GroupBy(c => c.DepartmentId)
            .Select(g => new DepartmentCountDto
            {
                DepartmentId = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderByDescending(d => d.Count)
            .ToListAsync();
    }

    // Compute average and median resolution time for resolved complaints
    public async Task<ResolutionTimeDto> GetResolutionTimeAsync()
    {
        var resolved = await _context.Complaints
            .Where(c => c.Status == ComplaintStatus.Resolved && c.UpdatedAt.HasValue)
            .Select(c => (c.UpdatedAt!.Value - c.CreatedAt).TotalHours)
            .ToListAsync();

        if (resolved.Count == 0)
        {
            return new ResolutionTimeDto { ResolvedCount = 0, AverageHours = 0, MedianHours = 0 };
        }

        resolved.Sort();
        var median = resolved.Count % 2 == 0
            ? (resolved[resolved.Count / 2 - 1] + resolved[resolved.Count / 2]) / 2
            : resolved[resolved.Count / 2];

        return new ResolutionTimeDto
        {
            ResolvedCount = resolved.Count,
            AverageHours = Math.Round(resolved.Average(), 2),
            MedianHours = Math.Round(median, 2)
        };
    }
}