using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.API.Services;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Controllers;

// Controller for managing student complaints
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private readonly IComplaintService _complaintService;
    private readonly ApplicationDbContext? _context;

    public ComplaintsController(IComplaintService complaintService, ApplicationDbContext? context = null)
    {
        _complaintService = complaintService;
        _context = context;
    }

    // GET api/complaints - Get all complaints (admin only)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<ComplaintResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _complaintService.GetAllComplaintsAsync();
        return Ok(result);
    }

    // GET api/complaints/{id} - Get single complaint by ID (admins or complaint owner)
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Student")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _complaintService.GetComplaintByIdAsync(id);
        if (result == null) return NotFound(new { message = $"Complaint with ID {id} was not found." });

        if (User.IsInRole("Admin"))
        {
            return Ok(result);
        }

        // Students may only view their own complaint details
        if (!await IsComplaintOwnerAsync(result))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to view this complaint." });
        }

        return Ok(result);
    }

    // GET api/complaints/student/{studentId} - Get complaints by student ID (admins or complaint owner)
    [HttpGet("student/{studentId:int}")]
    [Authorize(Roles = "Admin,Student")]
    [ProducesResponseType(typeof(IEnumerable<ComplaintResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByStudentId(int studentId)
    {
        if (!User.IsInRole("Admin"))
        {
            if (!await IsStudentIdOwnerAsync(studentId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to view another student's complaints." });
            }
        }

        var result = await _complaintService.GetComplaintsByStudentIdAsync(studentId);
        return Ok(result);
    }

    // GET api/complaints/department/{departmentId} - Get complaints by department ID
    [HttpGet("department/{departmentId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<ComplaintResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDepartmentId(int departmentId)
    {
        var result = await _complaintService.GetComplaintsByDepartmentIdAsync(departmentId);
        return Ok(result);
    }

    // POST api/complaints - Submit new complaint
    [HttpPost]
    [Authorize(Roles = "Admin,Student")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ComplaintCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _complaintService.CreateComplaintAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/complaints/{id}/status - Update complaint status (admin only)
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ComplaintStatusUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var changedBy = User.Identity?.Name ?? "Admin";
        var updated = await _complaintService.UpdateStatusAsync(id, dto.Status, dto.Comment, changedBy);
        if (updated == null) return NotFound(new { message = $"Complaint with ID {id} was not found." });

        return Ok(updated);
    }

    // GET api/complaints/{id}/history - Get status audit history for a complaint (admins or complaint owner)
    [HttpGet("{id:int}/history")]
    [Authorize(Roles = "Admin,Student")]
    [ProducesResponseType(typeof(IEnumerable<StatusHistoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(int id)
    {
        var complaint = await _complaintService.GetComplaintByIdAsync(id);
        if (complaint == null) return NotFound(new { message = $"Complaint with ID {id} was not found." });

        if (!User.IsInRole("Admin"))
        {
            if (!await IsComplaintOwnerAsync(complaint))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to view the history of this complaint." });
            }
        }

        var history = await _complaintService.GetStatusHistoryAsync(id);
        return Ok(history);
    }

    // PUT api/complaints/{id}/assign - Assign complaint to department/staff (admin only)
    [HttpPut("{id:int}/assign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(int id, [FromBody] ComplaintAssignDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var assigned = await _complaintService.AssignComplaintAsync(id, dto.DepartmentId, dto.AssignedToId);
        if (assigned == null) return NotFound(new { message = $"Complaint with ID {id} was not found." });

        return Ok(assigned);
    }

    // Check whether the currently authenticated student is the owner of this complaint
    private async Task<bool> IsComplaintOwnerAsync(ComplaintResponseDto complaint)
    {
        // 1. Check explicit student_id claim
        var studentIdClaim = User.FindFirstValue("student_id")
            ?? User.FindFirstValue("StudentId");
        if (!string.IsNullOrEmpty(studentIdClaim) && int.TryParse(studentIdClaim, out var claimStudentId))
        {
            if (claimStudentId == complaint.StudentId) return true;
        }

        // 2. Check NameIdentifier / sub claim if numeric student ID was stored
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var parsedUserId))
        {
            if (parsedUserId == complaint.StudentId) return true;
        }

        // 3. Check student email against user email claim
        var userEmail = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirst("email")?.Value;
        if (!string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(complaint.StudentEmail))
        {
            if (string.Equals(userEmail, complaint.StudentEmail, StringComparison.OrdinalIgnoreCase)) return true;
        }

        // 4. Check student name against username / name claim
        var userName = User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirst("name")?.Value;
        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(complaint.StudentName))
        {
            if (string.Equals(userName, complaint.StudentName, StringComparison.OrdinalIgnoreCase)) return true;
        }

        // 5. Check database student record against current user identity
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
    }

    // Check whether the currently authenticated student owns the specified studentId
    private async Task<bool> IsStudentIdOwnerAsync(int studentId)
    {
        var studentIdClaim = User.FindFirstValue("student_id")
            ?? User.FindFirstValue("StudentId");
        if (!string.IsNullOrEmpty(studentIdClaim) && int.TryParse(studentIdClaim, out var claimStudentId))
        {
            if (claimStudentId == studentId) return true;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var parsedUserId))
        {
            if (parsedUserId == studentId) return true;
        }

        var userEmail = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirst("email")?.Value;

        var userName = User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirst("name")?.Value;

        if (_context != null)
        {
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId);
            if (student != null)
            {
                if (!string.IsNullOrEmpty(userEmail) && string.Equals(student.Email, userEmail, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (!string.IsNullOrEmpty(userName) && (string.Equals(student.FullName, userName, StringComparison.OrdinalIgnoreCase) || string.Equals(student.IndexNumber, userName, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
        }

        return false;
    }
}
