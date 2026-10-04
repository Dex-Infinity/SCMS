using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCMS.API.Controllers;
using SCMS.API.DTOs;
using SCMS.API.Services;
using SCMS.Domain.Entities;
using Xunit;

namespace SCMS.API.Tests;

public class NotificationsControllerTests
{
    private static NotificationsController CreateControllerWithUser(
        INotificationService notificationService,
        string role,
        string userId = "1")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var controller = new NotificationsController(notificationService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetMyNotifications_ReturnsNotificationsForUser()
    {
        // Arrange
        var fakeService = new FakeNotificationService();
        fakeService.Notifications.Add(new NotificationResponseDto
        {
            Id = 1,
            UserId = "42",
            Title = "Status Update",
            Message = "Your complaint is InReview",
            IsRead = false
        });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.GetMyNotifications();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<NotificationResponseDto>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsCorrectCount()
    {
        // Arrange
        var fakeService = new FakeNotificationService();
        fakeService.Notifications.Add(new NotificationResponseDto { Id = 1, UserId = "42", IsRead = false });
        fakeService.Notifications.Add(new NotificationResponseDto { Id = 2, UserId = "42", IsRead = true });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.GetUnreadCount();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<NotificationUnreadCountDto>(okResult.Value);
        Assert.Equal(1, dto.UnreadCount);
    }

    [Fact]
    public async Task GetById_StudentForbiddenFromAccessingOtherUserNotification()
    {
        // Arrange
        var fakeService = new FakeNotificationService();
        fakeService.Notifications.Add(new NotificationResponseDto
        {
            Id = 5,
            UserId = "99",
            Title = "Private Alert"
        });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.GetById(5);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_MarksNotificationAsRead()
    {
        // Arrange
        var fakeService = new FakeNotificationService();
        fakeService.Notifications.Add(new NotificationResponseDto
        {
            Id = 7,
            UserId = "42",
            Title = "Test",
            IsRead = false
        });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.MarkAsRead(7);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<NotificationResponseDto>(okResult.Value);
        Assert.True(dto.IsRead);
    }

    [Fact]
    public async Task MarkAllAsRead_MarksAllForUser()
    {
        // Arrange
        var fakeService = new FakeNotificationService();
        fakeService.Notifications.Add(new NotificationResponseDto { Id = 1, UserId = "42", IsRead = false });
        fakeService.Notifications.Add(new NotificationResponseDto { Id = 2, UserId = "42", IsRead = false });

        var controller = CreateControllerWithUser(fakeService, role: "Student", userId: "42");

        // Act
        var result = await controller.MarkAllAsRead();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Create_AdminCanCreateNotification()
    {
        // Arrange
        var fakeService = new FakeNotificationService();
        var controller = CreateControllerWithUser(fakeService, role: "Admin", userId: "admin-1");

        var createDto = new NotificationCreateDto
        {
            UserId = "42",
            Title = "System Maintenance",
            Message = "Server downtime scheduled."
        };

        // Act
        var result = await controller.Create(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var dto = Assert.IsType<NotificationResponseDto>(createdResult.Value);
        Assert.Equal("System Maintenance", dto.Title);
    }
}

internal class FakeNotificationService : INotificationService
{
    public List<NotificationResponseDto> Notifications { get; } = new();

    public Task NotifyStatusChangeAsync(int complaintId, int studentId, string newStatus) =>
        Task.CompletedTask;

    public Task NotifyAssignmentAsync(int complaintId, int departmentId, int? assignedToId) =>
        Task.CompletedTask;

    public Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId) =>
        Task.FromResult(Enumerable.Empty<Notification>());

    public Task<IEnumerable<NotificationResponseDto>> GetNotificationsForUserAsync(
        string userId, int? studentId = null, bool? unreadOnly = null)
    {
        var targetIds = new HashSet<string> { userId };
        if (studentId.HasValue) targetIds.Add(studentId.Value.ToString());

        var query = Notifications.Where(n => targetIds.Contains(n.UserId));
        if (unreadOnly == true) query = query.Where(n => !n.IsRead);
        return Task.FromResult(query.AsEnumerable());
    }

    public Task<NotificationResponseDto?> GetNotificationByIdAsync(int id) =>
        Task.FromResult(Notifications.FirstOrDefault(n => n.Id == id));

    public Task<int> GetUnreadCountAsync(string userId, int? studentId = null)
    {
        var targetIds = new HashSet<string> { userId };
        if (studentId.HasValue) targetIds.Add(studentId.Value.ToString());
        return Task.FromResult(Notifications.Count(n => targetIds.Contains(n.UserId) && !n.IsRead));
    }

    public Task<NotificationResponseDto?> MarkAsReadAsync(int id, string? userId = null, int? studentId = null)
    {
        var item = Notifications.FirstOrDefault(n => n.Id == id);
        if (item != null) item.IsRead = true;
        return Task.FromResult(item);
    }

    public Task<int> MarkAllAsReadAsync(string userId, int? studentId = null)
    {
        var targetIds = new HashSet<string> { userId };
        if (studentId.HasValue) targetIds.Add(studentId.Value.ToString());

        var unread = Notifications.Where(n => targetIds.Contains(n.UserId) && !n.IsRead).ToList();
        foreach (var item in unread) item.IsRead = true;
        return Task.FromResult(unread.Count);
    }

    public Task<NotificationResponseDto> CreateNotificationAsync(NotificationCreateDto dto)
    {
        var item = new NotificationResponseDto
        {
            Id = Notifications.Count + 1,
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            ComplaintId = dto.ComplaintId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        Notifications.Add(item);
        return Task.FromResult(item);
    }

    public Task<bool> DeleteNotificationAsync(int id, string? userId = null, int? studentId = null)
    {
        var item = Notifications.FirstOrDefault(n => n.Id == id);
        if (item == null) return Task.FromResult(false);
        Notifications.Remove(item);
        return Task.FromResult(true);
    }
}
