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
    private readonly ApplicationDbContext? _dbContext;
    private readonly IComplaintRepository? _complaintRepository;
    private readonly IAttachmentRepository? _attachmentRepository;

    public AttachmentsController(
        IAttachmentService attachmentService,
        ApplicationDbContext? dbContext = null,
        IComplaintRepository? complaintRepository = null,
        IAttachmentRepository? attachmentRepository = null)
    {
        _attachmentService = attachmentService;
        _dbContext = dbContext;
        _complaintRepository = complaintRepository;
        _attachmentRepository = attachmentRepository;
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
        int? complaintId = null;
        if (_attachmentRepository != null)
        {
            var meta = await _attachmentRepository.GetByIdAsync(id);
            if (meta == null)
            {
                return NotFound(new { message = $"Attachment with ID {id} was not found." });
            }
            complaintId = meta.ComplaintId;
        }
        else if (_dbContext != null)
        {
            complaintId = await _dbContext.Attachments
                .Where(attachment => attachment.Id == id)
                .Select(attachment => (int?)attachment.ComplaintId)
                .FirstOrDefaultAsync();
            if (!complaintId.HasValue)
            {
                return NotFound(new { message = $"Attachment with ID {id} was not found." });
            }
        }

        if (complaintId.HasValue)
        {
            var accessError = await EnsureComplaintAccessAsync(complaintId.Value);
            if (accessError != null) return accessError;
        }

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
        int? complaintId = null;
        if (_attachmentRepository != null)
        {
            var meta = await _attachmentRepository.GetByIdAsync(id);
            if (meta == null)
            {
                return NotFound(new { message = $"Attachment with ID {id} was not found." });
            }
            complaintId = meta.ComplaintId;
        }
        else if (_dbContext != null)
        {
            complaintId = await _dbContext.Attachments
                .Where(attachment => attachment.Id == id)
                .Select(attachment => (int?)attachment.ComplaintId)
                .FirstOrDefaultAsync();
            if (!complaintId.HasValue)
            {
                return NotFound(new { message = $"Attachment with ID {id} was not found." });
            }
        }

        if (complaintId.HasValue)
        {
            var accessError = await EnsureComplaintAccessAsync(complaintId.Value);
            if (accessError != null) return accessError;
        }

        var deleted = await _attachmentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Attachment with ID {id} was not found." });
        }

        return NoContent();
    }

    private async Task<IActionResult?> EnsureComplaintAccessAsync(int complaintId)
    {
        if (User.IsInRole("Admin")) return null;

        int? studentId = null;
        if (_complaintRepository != null)
        {
            var comp = await _complaintRepository.GetByIdAsync(complaintId);
            if (comp == null)
            {
                return NotFound(new { message = $"Complaint with ID {complaintId} was not found." });
            }
            studentId = comp.StudentId;
        }
        else if (_dbContext != null)
        {
            studentId = await _dbContext.Complaints
                .Where(complaint => complaint.Id == complaintId)
                .Select(complaint => (int?)complaint.StudentId)
                .FirstOrDefaultAsync();
            if (!studentId.HasValue)
            {
                return NotFound(new { message = $"Complaint with ID {complaintId} was not found." });
            }
        }

        if (!studentId.HasValue)
        {
            return null;
        }

        var currentStudentId = User.FindFirstValue("student_id")
            ?? User.FindFirstValue("StudentId");
        if (int.TryParse(currentStudentId, out var parsedStudentId) && parsedStudentId == studentId.Value)
        {
            return null;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (int.TryParse(userId, out var parsedUserId) && parsedUserId == studentId.Value)
        {
            return null;
        }

        var userEmail = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirst("email")?.Value;
        if (!string.IsNullOrEmpty(userEmail) && _dbContext != null)
        {
            var student = await _dbContext.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId.Value);
            if (student != null && string.Equals(student.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
        }

        return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to access attachments for this complaint." });
    }
}