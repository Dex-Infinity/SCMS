using Microsoft.AspNetCore.Mvc;
using SCMS.API.DTOs;
using SCMS.API.Services;

namespace SCMS.API.Controllers;

// Controller for managing student complaints
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ComplaintsController : ControllerBase
{
    private readonly IComplaintService _complaintService;

    // Inject complaint service
    public ComplaintsController(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    // GET api/complaints - Get all complaints
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ComplaintResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _complaintService.GetAllComplaintsAsync();
        return Ok(result);
    }

    // GET api/complaints/{id} - Get single complaint by ID
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _complaintService.GetComplaintByIdAsync(id);
        if (result == null) return NotFound(new { message = $"Complaint with ID {id} was not found." });

        return Ok(result);
    }

    // GET api/complaints/student/{studentId} - Get complaints by student ID
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(IEnumerable<ComplaintResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStudentId(string studentId)
    {
        var result = await _complaintService.GetComplaintsByStudentIdAsync(studentId);
        return Ok(result);
    }

    // POST api/complaints - Submit new complaint
    [HttpPost]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ComplaintCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _complaintService.CreateComplaintAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/complaints/{id}/status - Update complaint status
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ComplaintResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ComplaintStatusUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _complaintService.UpdateStatusAsync(id, dto.Status);
        if (updated == null) return NotFound(new { message = $"Complaint with ID {id} was not found." });

        return Ok(updated);
    }

    // PUT api/complaints/{id}/assign - Assign complaint to department/staff
    [HttpPut("{id:int}/assign")]
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
