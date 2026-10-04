using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.API.Repositories;
using SCMS.API.Services;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Controllers;

// Controller for uploading, listing, downloading, and removing supporting documents
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Student")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;
    private readonly IComplaintRepository? _complaintRepository;
    private readonly IAttachmentRepository? _attachmentRepository;
    private readonly ApplicationDbContext? _context;

    public AttachmentsController(
        IAttachmentService attachmentService,
        IComplaintRepository? complaintRepository = null,
        IAttachmentRepository? attachmentRepository = null,
        ApplicationDbContext? context = null)
    {
        _attachmentService = attachmentService;
        _complaintRepository = complaintRepository;
        _attachmentRepository = attachmentRepository;
        _context = context;
    private readonly ApplicationDbContext _dbContext;

    // Inject attachment service
    public AttachmentsController(IAttachmentService attachmentService, ApplicationDbContext dbContext)
    {
        _attachmentService = attachmentService;
        _dbContext = dbContext;
    }

    // POST api/attachments/complaints/{complaintId} - Upload a supporting document
    [HttpPost("complaints/{complaintId:int}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AttachmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(int complaintId, [FromForm] IFormFile file)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!User.IsInRole("Admin"))
        {
            if (!await IsComplaintOwnerAsync(complaintId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to upload attachments to this complaint." });
            }
        }
        var accessError = await EnsureComplaintAccessAsync(complaintId);
        if (accessError != null) return accessError;

        var uploadedBy = User.Identity?.Name ?? "unknown";

        try
        {
            var result = await _attachmentService.UploadAsync(complaintId, file, uploadedBy);
            if (result == null)
            {
                return NotFound(new { message = $"Complaint with ID {complaintId} was not found." });
            }

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET api/attachments/complaints/{complaintId} - List attachments for a complaint
    [HttpGet("complaints/{complaintId:int}")]
    [ProducesResponseType(typeof(IEnumerable<AttachmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByComplaint(int complaintId)
    {
        if (!User.IsInRole("Admin"))
        {
            if (!await IsComplaintOwnerAsync(complaintId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to view attachments for this complaint." });
            }
        }
        var accessError = await EnsureComplaintAccessAsync(complaintId);
        if (accessError != null) return accessError;

        var result = await _attachmentService.GetComplaintAttachmentsAsync(complaintId);
        return Ok(result);
    }

    // GET api/attachments/{id} - Download an attachment
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int id)
    {
        if (!User.IsInRole("Admin") && _attachmentRepository != null)
        {
            var meta = await _attachmentRepository.GetByIdAsync(id);
            if (meta == null)
            {
                return NotFound(new { message = $"Attachment with ID {id} was not found." });
            }

            if (!await IsComplaintOwnerAsync(meta.ComplaintId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to download this attachment." });
            }
        }

        var complaintId = await _dbContext.Attachments
            .Where(attachment => attachment.Id == id)
            .Select(attachment => (int?)attachment.ComplaintId)
            .FirstOrDefaultAsync();
        if (!complaintId.HasValue)
        {
            return NotFound(new { message = $"Attachment with ID {id} was not found." });
        }

        var accessError = await EnsureComplaintAccessAsync(complaintId.Value);
        if (accessError != null) return accessError;

        var result = await _attachmentService.DownloadAsync(id);
        if (result == null)
        {
            return NotFound(new { message = $"Attachment with ID {id} was not found." });
        }

        return File(result.Stream, result.ContentType, result.Attachment.FileName);
    }

    // DELETE api/attachments/{id} - Remove an attachment
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!User.IsInRole("Admin") && _attachmentRepository != null)
        {
            var meta = await _attachmentRepository.GetByIdAsync(id);
            if (meta == null)
            {
                return NotFound(new { message = $"Attachment with ID {id} was not found." });
            }

            if (!await IsComplaintOwnerAsync(meta.ComplaintId) && !string.Equals(meta.UploadedBy, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to delete this attachment." });
            }
        }

        var complaintId = await _dbContext.Attachments
            .Where(attachment => attachment.Id == id)
            .Select(attachment => (int?)attachment.ComplaintId)
            .FirstOrDefaultAsync();
        if (!complaintId.HasValue)
        {
            return NotFound(new { message = $"Attachment with ID {id} was not found." });
        }

        var accessError = await EnsureComplaintAccessAsync(complaintId.Value);
        if (accessError != null) return accessError;

        var deleted = await _attachmentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Attachment with ID {id} was not found." });
        }

        return NoContent();
    }

    // Check whether current user owns the specified complaint
    private async Task<bool> IsComplaintOwnerAsync(int complaintId)
    {
        if (_complaintRepository == null) return true;

        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null) return false;

        var studentIdClaim = User.FindFirstValue("student_id")
            ?? User.FindFirstValue("StudentId");
        if (!string.IsNullOrEmpty(studentIdClaim) && int.TryParse(studentIdClaim, out var claimStudentId))
        {
            if (claimStudentId == complaint.StudentId) return true;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var parsedUserId))
        {
            if (parsedUserId == complaint.StudentId) return true;
        }

        var userEmail = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirst("email")?.Value;
        if (!string.IsNullOrEmpty(userEmail) && complaint.Student != null)
        {
            if (string.Equals(userEmail, complaint.Student.Email, StringComparison.OrdinalIgnoreCase)) return true;
        }

        var userName = User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirst("name")?.Value;
        if (!string.IsNullOrEmpty(userName) && complaint.Student != null)
        {
            if (string.Equals(userName, complaint.Student.FullName, StringComparison.OrdinalIgnoreCase)) return true;
        }

        if (_context != null)
        {
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == complaint.StudentId);
            if (student != null)
            {
                if (!string.IsNullOrEmpty(userEmail) && string.Equals(student.Email, userEmail, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (!string.IsNullOrEmpty(userName) && (string.Equals(student.FullName, userName, StringComparison.OrdinalIgnoreCase) || string.Equals(student.IndexNumber, userName, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
        }

        return false;
    private async Task<IActionResult?> EnsureComplaintAccessAsync(int complaintId)
    {
        var studentId = await _dbContext.Complaints
            .Where(complaint => complaint.Id == complaintId)
            .Select(complaint => (int?)complaint.StudentId)
            .FirstOrDefaultAsync();
        if (!studentId.HasValue)
        {
            return NotFound(new { message = $"Complaint with ID {complaintId} was not found." });
        }

        if (User.IsInRole("Admin")) return null;

        var currentStudentId = User.FindFirstValue("student_id");
        return int.TryParse(currentStudentId, out var parsedStudentId) && parsedStudentId == studentId.Value
            ? null
            : Forbid();
    }
}