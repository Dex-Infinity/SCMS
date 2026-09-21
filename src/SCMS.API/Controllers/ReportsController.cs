using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCMS.API.DTOs;
using SCMS.API.Services;

namespace SCMS.API.Controllers;

// Controller for reporting and analytics dashboards
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    // Inject analytics service
    public ReportsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    // GET api/reports/summary - Full dashboard summary
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ReportsSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _analyticsService.GetSummaryAsync();
        return Ok(result);
    }

    // GET api/reports/by-status - Counts grouped by complaint status
    [HttpGet("by-status")]
    [ProducesResponseType(typeof(IEnumerable<StatusCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus()
    {
        var result = await _analyticsService.CountByStatusAsync();
        return Ok(result);
    }

    // GET api/reports/by-department - Counts grouped by department
    [HttpGet("by-department")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDepartment()
    {
        var result = await _analyticsService.CountByDepartmentAsync();
        return Ok(result);
    }

    // GET api/reports/resolution-time - Average and median complaint resolution time
    [HttpGet("resolution-time")]
    [ProducesResponseType(typeof(ResolutionTimeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResolutionTime()
    {
        var result = await _analyticsService.GetResolutionTimeAsync();
        return Ok(result);
    }
}