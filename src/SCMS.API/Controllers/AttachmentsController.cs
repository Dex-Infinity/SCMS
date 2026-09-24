using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCMS.API.DTOs;
using SCMS.API.Services;

namespace SCMS.API.Controllers;

// Controller for uploading and downloading supporting documents
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Student")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    // Inject attachment service
    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
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
        var result = await _attachmentService.GetComplaintAttachmentsAsync(complaintId);
        return Ok(result);
    }

    // GET api/attachments/{id} - Download an attachment
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int id)
    {
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
        var deleted = await _attachmentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Attachment with ID {id} was not found." });
        }

        return NoContent();
    }
}