using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCMS.API.Controllers;
using SCMS.API.DTOs;
using SCMS.API.Repositories;
using SCMS.API.Services;
using SCMS.Domain.Entities;
using SCMS.Domain.Enums;
using Xunit;

namespace SCMS.API.Tests;

public class AttachmentsControllerTests
{
    private static AttachmentsController CreateControllerWithUser(
        IAttachmentService attachmentService,
        IComplaintRepository? complaintRepo = null,
        IAttachmentRepository? attachmentRepo = null,
        string role = "Student",
        string userId = "42",
        string? email = null)
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

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        return new AttachmentsController(attachmentService, complaintRepo, attachmentRepo)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }

    [Fact]
    public async Task GetByComplaint_Forbidden_WhenStudentDoesNotOwnComplaint()
    {
        // Arrange
        var fakeAttachmentService = new FakeAttachmentService();
        var fakeComplaintRepo = new FakeComplaintRepoForAttachments();
        fakeComplaintRepo.Complaints.Add(new Complaint
        {
            Id = 1,
            StudentId = 99 // different student
        });

        var controller = CreateControllerWithUser(fakeAttachmentService, fakeComplaintRepo, role: "Student", userId: "42");

        // Act
        var result = await controller.GetByComplaint(1);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByComplaint_OwnerCanViewAttachments()
    {
        // Arrange
        var fakeAttachmentService = new FakeAttachmentService();
        fakeAttachmentService.Attachments.Add(new AttachmentResponseDto
        {
            Id = 1,
            ComplaintId = 1,
            FileName = "evidence.pdf"
        });

        var fakeComplaintRepo = new FakeComplaintRepoForAttachments();
        fakeComplaintRepo.Complaints.Add(new Complaint
        {
            Id = 1,
            StudentId = 42
        });

        var controller = CreateControllerWithUser(fakeAttachmentService, fakeComplaintRepo, role: "Student", userId: "42");

        // Act
        var result = await controller.GetByComplaint(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<AttachmentResponseDto>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task Download_ReturnsNotFound_WhenAttachmentDoesNotExist()
    {
        // Arrange
        var fakeAttachmentService = new FakeAttachmentService();
        var controller = CreateControllerWithUser(fakeAttachmentService, role: "Student", userId: "42");

        // Act
        var result = await controller.Download(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        // Arrange
        var fakeAttachmentService = new FakeAttachmentService();
        fakeAttachmentService.Attachments.Add(new AttachmentResponseDto
        {
            Id = 1,
            ComplaintId = 1,
            FileName = "doc.pdf"
        });

        var controller = CreateControllerWithUser(fakeAttachmentService, role: "Admin", userId: "admin-1");

        // Act
        var result = await controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}

internal class FakeAttachmentService : IAttachmentService
{
    public List<AttachmentResponseDto> Attachments { get; } = new();

    public Task<AttachmentResponseDto?> UploadAsync(int complaintId, IFormFile file, string uploadedBy)
    {
        var dto = new AttachmentResponseDto
        {
            Id = Attachments.Count + 1,
            ComplaintId = complaintId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        };
        Attachments.Add(dto);
        return Task.FromResult<AttachmentResponseDto?>(dto);
    }

    public Task<IEnumerable<AttachmentResponseDto>> GetComplaintAttachmentsAsync(int complaintId) =>
        Task.FromResult(Attachments.Where(a => a.ComplaintId == complaintId));

    public Task<AttachmentDownloadResult?> DownloadAsync(int id) =>
        Task.FromResult<AttachmentDownloadResult?>(null);

    public Task<bool> DeleteAsync(int id)
    {
        var item = Attachments.FirstOrDefault(a => a.Id == id);
        if (item == null) return Task.FromResult(false);
        Attachments.Remove(item);
        return Task.FromResult(true);
    }
}

internal class FakeComplaintRepoForAttachments : IComplaintRepository
{
    public List<Complaint> Complaints { get; } = new();

    public Task<IEnumerable<Complaint>> GetAllAsync() => Task.FromResult<IEnumerable<Complaint>>(Complaints);
    public Task<Complaint?> GetByIdAsync(int id) => Task.FromResult(Complaints.FirstOrDefault(c => c.Id == id));
    public Task<IEnumerable<Complaint>> GetByStudentIdAsync(int studentId) => Task.FromResult(Complaints.Where(c => c.StudentId == studentId));
    public Task<IEnumerable<Complaint>> GetByDepartmentIdAsync(int departmentId) => Task.FromResult(Complaints.Where(c => c.DepartmentId == departmentId));
    public Task<Complaint> CreateAsync(Complaint complaint) { Complaints.Add(complaint); return Task.FromResult(complaint); }
    public Task<Complaint?> UpdateStatusAsync(int id, ComplaintStatus status) => Task.FromResult<Complaint?>(null);
    public Task<Complaint?> AssignAsync(int id, int departmentId, int? assignedToId) => Task.FromResult<Complaint?>(null);
    public Task<IEnumerable<StatusHistory>> GetStatusHistoryAsync(int complaintId) => Task.FromResult<IEnumerable<StatusHistory>>(new List<StatusHistory>());
    public Task AddStatusHistoryAsync(StatusHistory history) => Task.CompletedTask;
}
