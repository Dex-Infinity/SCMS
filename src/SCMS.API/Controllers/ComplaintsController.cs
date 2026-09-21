using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCMS.API.DTOs;
using SCMS.API.Services;

namespace SCMS.API.Controllers;

// Controller for managing student complaints
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private readonly IComplaintService _complaintService;

    // Inject complaint service
    public ComplaintsController(IComplaintService complaintService)
    {
        _complaintService = complaintService;
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

    // GET api/complaints/{id} - Get single complaint by ID (admin only)
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _complaintService.GetComplaintByIdAsync(id);
        if (result == null) return NotFound(new { message = $"Complaint with ID {id} was not found." });

        return Ok(result);
    }

    // GET api/complaints/student/{studentId} - Get complaints by student ID (student/admin)
    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Admin,Student")]
    [ProducesResponseType(typeof(IEnumerable<ComplaintResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStudentId(string studentId)
    {
        var result = await _complaintService.GetComplaintsByStudentIdAsync(studentId);
        return Ok(result);
    }

    // POST api/complaints - Submit new complaint (student/admin)
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

        var updated = await _complaintService.UpdateStatusAsync(id, dto.Status);
        if (updated == null) return NotFound(new { message = $"Complaint with ID {id} was not found." });

        return Ok(updated);
    }

    // PUT api/complaints/{id}/assign - Assign complaint to department/staff (admin only)
    [HttpPut("{id:int}/assign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(int id, [FromBody] ComplaintAssignDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var assigned = await _complaintService.AssignComplaintAsync(id, dto.DepartmentId, dto.AssignedTo);
        if (assigned == null) return NotFound(new { message = $"Complaint with ID {id} was not found." });

        return Ok(assigned);
    }
}
