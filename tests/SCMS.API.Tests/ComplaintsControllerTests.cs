using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCMS.API.Controllers;
using SCMS.API.DTOs;
using SCMS.API.Services;
using SCMS.Domain.Enums;
using Xunit;

namespace SCMS.API.Tests;

public class ComplaintsControllerTests
{
    private static ComplaintsController CreateControllerWithUser(
        IComplaintService complaintService,
        string role,
        string userId = "1",
        string? email = null,
        string? name = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role)
        };

        if (email != null)
        {
            claims.Add(new(ClaimTypes.Email, email));
        }

        if (name != null)
        {
            claims.Add(new(ClaimTypes.Name, name));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var controller = new ComplaintsController(complaintService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetById_AdminCanAccessAnyComplaint()
    {
        // Arrange
        var fakeService = new FakeComplaintService();
        fakeService.Complaints.Add(new ComplaintResponseDto
        {
            Id = 10,
            Title = "Test Complaint",
            StudentId = 99, // Belongs to student 99
            StudentEmail = "other@student.com"
        });

        var controller = CreateControllerWithUser(fakeService, role: "Admin", userId: "admin-1");

        // Act
        var result = await controller.GetById(10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<ComplaintResponseDto>(okResult.Value);
        Assert.Equal(10, dto.Id);
    }

    [Fact]
    public async Task GetById_StudentCanAccessOwnComplaint()
    {
        // Arrange
        var fakeService = new FakeComplaintService();
        fakeService.Complaints.Add(new ComplaintResponseDto
        {
            Id = 10,
            Title = "My Complaint",
            StudentId = 42,
            StudentEmail = "student@test.com"
        });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42", email: "student@test.com");

        // Act
        var result = await controller.GetById(10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<ComplaintResponseDto>(okResult.Value);
        Assert.Equal(10, dto.Id);
    }

    [Fact]
    public async Task GetById_StudentForbiddenFromAccessingOtherStudentsComplaint()
    {
        // Arrange
        var fakeService = new FakeComplaintService();
        fakeService.Complaints.Add(new ComplaintResponseDto
        {
            Id = 10,
            Title = "Other Student's Complaint",
            StudentId = 99,
            StudentEmail = "other@student.com"
        });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42", email: "student@test.com");

        // Act
        var result = await controller.GetById(10);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundWhenComplaintDoesNotExist()
    {
        // Arrange
        var fakeService = new FakeComplaintService();
        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetByStudentId_StudentForbiddenFromViewingOtherStudentList()
    {
        // Arrange
        var fakeService = new FakeComplaintService();
        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.GetByStudentId(99);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByStudentId_StudentCanViewOwnComplaints()
    {
        // Arrange
        var fakeService = new FakeComplaintService();
        fakeService.Complaints.Add(new ComplaintResponseDto
        {
            Id = 1,
            Title = "My Complaint",
            StudentId = 42
        });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.GetByStudentId(42);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<ComplaintResponseDto>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetHistory_StudentForbidden_WhenNotOwner()
    {
        // Arrange
        var fakeService = new FakeComplaintService();
        fakeService.Complaints.Add(new ComplaintResponseDto
        {
            Id = 1,
            StudentId = 99
        });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.GetHistory(1);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetHistory_OwnerCanViewHistory()
    {
        // Arrange
        var fakeService = new FakeComplaintService();
        fakeService.Complaints.Add(new ComplaintResponseDto
        {
            Id = 1,
            StudentId = 42
        });
        fakeService.Histories.Add(new StatusHistoryResponseDto
        {
            Id = 1,
            ComplaintId = 1,
            Status = ComplaintStatus.Pending,
            Comment = "Complaint submitted."
        });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.GetHistory(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<StatusHistoryResponseDto>>(okResult.Value);
        Assert.Single(list);
    }
}

internal class FakeComplaintService : IComplaintService
{
    public List<ComplaintResponseDto> Complaints { get; } = new();
    public List<StatusHistoryResponseDto> Histories { get; } = new();

    public Task<IEnumerable<ComplaintResponseDto>> GetAllComplaintsAsync() =>
        Task.FromResult<IEnumerable<ComplaintResponseDto>>(Complaints);

    public Task<ComplaintResponseDto?> GetComplaintByIdAsync(int id) =>
        Task.FromResult(Complaints.FirstOrDefault(c => c.Id == id));

    public Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByStudentIdAsync(int studentId) =>
        Task.FromResult(Complaints.Where(c => c.StudentId == studentId));

    public Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByDepartmentIdAsync(int departmentId) =>
        Task.FromResult(Complaints.Where(c => c.DepartmentId == departmentId));

    public Task<ComplaintResponseDto> CreateComplaintAsync(ComplaintCreateDto createDto)
    {
        var dto = new ComplaintResponseDto
        {
            Id = Complaints.Count + 1,
            Title = createDto.Title,
            Description = createDto.Description,
            StudentId = createDto.StudentId,
            DepartmentId = createDto.DepartmentId,
            Status = ComplaintStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        Complaints.Add(dto);
        return Task.FromResult(dto);
    }

    public Task<ComplaintResponseDto?> UpdateStatusAsync(int id, ComplaintStatus status, string? comment = null, string? changedBy = null)
    {
        var item = Complaints.FirstOrDefault(c => c.Id == id);
        if (item != null) item.Status = status;
        return Task.FromResult(item);
    }

    public Task<ComplaintResponseDto?> AssignComplaintAsync(int id, int departmentId, int? assignedToId)
    {
        var item = Complaints.FirstOrDefault(c => c.Id == id);
        if (item != null)
        {
            item.DepartmentId = departmentId;
            item.AssignedToId = assignedToId;
        }
        return Task.FromResult(item);
    }

    public Task<IEnumerable<StatusHistoryResponseDto>> GetStatusHistoryAsync(int complaintId) =>
        Task.FromResult(Histories.Where(h => h.ComplaintId == complaintId));
}
