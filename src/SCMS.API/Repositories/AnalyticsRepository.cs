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
        var departments = await _context.Departments
            .AsNoTracking()
            .ToDictionaryAsync(d => d.Id, d => d.Name);

        var grouped = await _context.Complaints
            .GroupBy(c => c.DepartmentId)
            .Select(g => new
            {
                DepartmentId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(d => d.Count)
            .ToListAsync();

        return grouped.Select(g => new DepartmentCountDto
        {
            DepartmentId = g.DepartmentId.HasValue ? g.DepartmentId.Value.ToString() : null,
            DepartmentName = g.DepartmentId.HasValue && departments.TryGetValue(g.DepartmentId.Value, out var name)
                ? name
                : "Unassigned",
            Count = g.Count
        }).ToList();
    }

    // Compute average and median resolution time for resolved complaints
    public async Task<ResolutionTimeDto> GetResolutionTimeAsync()
    {
        // Project timestamps first to guarantee database-agnostic translation on SQL Server
        var resolvedRecords = await _context.Complaints
            .Where(c => c.Status == ComplaintStatus.Resolved && c.UpdatedAt.HasValue)
            .Select(c => new { c.CreatedAt, UpdatedAt = c.UpdatedAt!.Value })
            .ToListAsync();

        if (resolvedRecords.Count == 0)
        {
            return new ResolutionTimeDto { ResolvedCount = 0, AverageHours = 0, MedianHours = 0 };
        }

        var resolved = resolvedRecords
            .Select(c => (c.UpdatedAt - c.CreatedAt).TotalHours)
            .Where(h => h >= 0)
            .ToList();

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