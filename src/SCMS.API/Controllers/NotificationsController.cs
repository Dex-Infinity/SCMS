using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.API.Services;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Controllers;

// Controller for viewing, marking as read, and managing user notifications
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ApplicationDbContext? _context;

    public NotificationsController(INotificationService notificationService, ApplicationDbContext? context = null)
    {
        _notificationService = notificationService;
        _context = context;
    }

    // GET api/notifications - Get notifications for the currently authenticated user
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool? unreadOnly = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Unable to identify the current user." });

        var studentId = await GetCurrentStudentIdAsync();
        var notifications = await _notificationService.GetNotificationsForUserAsync(userId, studentId, unreadOnly);
        return Ok(notifications);
    }

    // GET api/notifications/unread-count - Get count of unread notifications for current user
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(NotificationUnreadCountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Unable to identify the current user." });

        var studentId = await GetCurrentStudentIdAsync();
        var count = await _notificationService.GetUnreadCountAsync(userId, studentId);
        return Ok(new NotificationUnreadCountDto { UnreadCount = count });
    }

    // GET api/notifications/{id} - Get a single notification by ID (admin or notification owner)
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null) return NotFound(new { message = $"Notification with ID {id} was not found." });

        if (!User.IsInRole("Admin"))
        {
            var userId = GetCurrentUserId();
            var studentId = await GetCurrentStudentIdAsync();
            if (!IsNotificationOwner(notification, userId, studentId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to view this notification." });
            }
        }

        return Ok(notification);
    }

    // PUT api/notifications/{id}/read - Mark single notification as read
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null) return NotFound(new { message = $"Notification with ID {id} was not found." });

        var isAdmin = User.IsInRole("Admin");
        var userId = GetCurrentUserId();
        var studentId = await GetCurrentStudentIdAsync();

        if (!isAdmin && !IsNotificationOwner(notification, userId, studentId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to update this notification." });
        }

        var updated = await _notificationService.MarkAsReadAsync(id, isAdmin ? null : userId, studentId);
        return Ok(updated);
    }

    // PUT api/notifications/read-all - Mark all notifications for the current user as read
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Unable to identify the current user." });

        var studentId = await GetCurrentStudentIdAsync();
        var count = await _notificationService.MarkAllAsReadAsync(userId, studentId);
        return Ok(new { count, message = $"{count} notification(s) marked as read." });
    }

    // POST api/notifications - Manually create a notification (admin only)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (_context != null)
        {
            var isIdentityUser = await _context.Users.AnyAsync(user => user.Id == dto.UserId);
            var isStudent = int.TryParse(dto.UserId, out var studentId)
                && await _context.Students.AnyAsync(student => student.Id == studentId);
            if (!isIdentityUser && !isStudent)
            {
                return BadRequest(new { message = "The target user was not found." });
            }

            if (dto.ComplaintId.HasValue && !await _context.Complaints.AnyAsync(complaint => complaint.Id == dto.ComplaintId.Value))
            {
                return BadRequest(new { message = "The complaint was not found." });
            }
        }

        var created = await _notificationService.CreateNotificationAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // DELETE api/notifications/{id} - Delete a notification (admin or notification owner)
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null) return NotFound(new { message = $"Notification with ID {id} was not found." });

        var isAdmin = User.IsInRole("Admin");
        var userId = GetCurrentUserId();
        var studentId = await GetCurrentStudentIdAsync();

        if (!isAdmin && !IsNotificationOwner(notification, userId, studentId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to delete this notification." });
        }

        var deleted = await _notificationService.DeleteNotificationAsync(id, isAdmin ? null : userId, studentId);
        if (!deleted) return NotFound(new { message = $"Notification with ID {id} was not found." });

        return NoContent();
    }

    // GET api/notifications/user/{userId} - Get notifications for any user (admin only)
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserId(string userId, [FromQuery] bool? unreadOnly = null)
    {
        int? studentId = int.TryParse(userId, out var parsedStudentId) ? parsedStudentId : null;
        if (!studentId.HasValue && _context != null)
        {
            var email = await _context.Users
                .Where(user => user.Id == userId)
                .Select(user => user.Email)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(email))
            {
                studentId = await _context.Students
                    .Where(student => student.Email == email)
                    .Select(student => (int?)student.Id)
                    .FirstOrDefaultAsync();
            }
        }

        var notifications = await _notificationService.GetNotificationsForUserAsync(userId, studentId, unreadOnly);
        return Ok(notifications);
    }

    // Helper: Retrieve current user's ID claim
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }

    // Helper: Determine student ID for current user
    private async Task<int?> GetCurrentStudentIdAsync()
    {
        var studentIdClaim = User.FindFirstValue("student_id")
            ?? User.FindFirstValue("StudentId");
        if (!string.IsNullOrEmpty(studentIdClaim) && int.TryParse(studentIdClaim, out var claimStudentId))
        {
            return claimStudentId;
        }

        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var parsedUserId))
        {
            return parsedUserId;
        }

        var userEmail = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirst("email")?.Value;

        var userName = User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirst("name")?.Value;

        if (_context != null)
        {
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s =>
                (!string.IsNullOrEmpty(userEmail) && s.Email == userEmail) ||
                (!string.IsNullOrEmpty(userName) && (s.FullName == userName || s.IndexNumber == userName)));

            if (student != null)
            {
                return student.Id;
            }
        }

        return null;
    }

    // Helper: Verify whether notification belongs to user or student
    private static bool IsNotificationOwner(NotificationResponseDto notification, string? userId, int? studentId)
    {
        if (!string.IsNullOrEmpty(userId) && string.Equals(notification.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (studentId.HasValue && string.Equals(notification.UserId, studentId.Value.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
