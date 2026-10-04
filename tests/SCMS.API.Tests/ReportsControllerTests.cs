using Microsoft.AspNetCore.Mvc;
using SCMS.API.Controllers;
using SCMS.API.DTOs;
using SCMS.API.Services;
using Xunit;

namespace SCMS.API.Tests;

public class ReportsControllerTests
{
    [Fact]
    public async Task GetSummary_ReturnsOkWithDashboardSummary()
    {
        // Arrange
        var fakeAnalytics = new FakeAnalyticsService();
        var controller = new ReportsController(fakeAnalytics);

        // Act
        var result = await controller.GetSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var summary = Assert.IsType<ReportsSummaryDto>(okResult.Value);
        Assert.Equal(25, summary.Total);
        Assert.Equal(10, summary.Pending);
    }

    [Fact]
    public async Task GetByStatus_ReturnsOkWithStatusCounts()
    {
        // Arrange
        var fakeAnalytics = new FakeAnalyticsService();
        var controller = new ReportsController(fakeAnalytics);

        // Act
        var result = await controller.GetByStatus();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<StatusCountDto>>(okResult.Value);
        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task GetByDepartment_ReturnsOkWithDepartmentCounts()
    {
        // Arrange
        var fakeAnalytics = new FakeAnalyticsService();
        var controller = new ReportsController(fakeAnalytics);

        // Act
        var result = await controller.GetByDepartment();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<DepartmentCountDto>>(okResult.Value);
        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task GetResolutionTime_ReturnsOkWithDurationStats()
    {
        // Arrange
        var fakeAnalytics = new FakeAnalyticsService();
        var controller = new ReportsController(fakeAnalytics);

        // Act
        var result = await controller.GetResolutionTime();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var stats = Assert.IsType<ResolutionTimeDto>(okResult.Value);
        Assert.Equal(15, stats.ResolvedCount);
        Assert.Equal(24.5, stats.AverageHours);
    }
}

internal class FakeAnalyticsService : IAnalyticsService
{
    public Task<ReportsSummaryDto> GetSummaryAsync() => Task.FromResult(new ReportsSummaryDto
    {
        Total = 25,
        Pending = 10,
        UnderReview = 5,
        Assigned = 3,
        Resolved = 5,
        Rejected = 2,
        ResolutionTime = new ResolutionTimeDto { ResolvedCount = 5, AverageHours = 12.5, MedianHours = 10.0 }
    });

    public Task<List<StatusCountDto>> CountByStatusAsync() => Task.FromResult(new List<StatusCountDto>
    {
        new() { Status = "Pending", Count = 10 },
        new() { Status = "Resolved", Count = 5 }
    });

    public Task<List<DepartmentCountDto>> CountByDepartmentAsync() => Task.FromResult(new List<DepartmentCountDto>
    {
        new() { DepartmentId = "1", DepartmentName = "Computer Science", Count = 15 }
    });

    public Task<ResolutionTimeDto> GetResolutionTimeAsync() => Task.FromResult(new ResolutionTimeDto
    {
        ResolvedCount = 15,
        AverageHours = 24.5,
        MedianHours = 20.0
    });
}
