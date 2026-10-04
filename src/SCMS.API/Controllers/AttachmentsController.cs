using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.API.Services;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Controllers;

// Controller for uploading and downloading supporting documents
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Student")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int id)
    {
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
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