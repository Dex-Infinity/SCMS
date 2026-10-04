using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Controllers;

// Controller for retrieving academic and administrative departments
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DepartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET api/departments - Get list of all departments
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DepartmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                Description = d.Description
            })
            .ToListAsync();

        return Ok(departments);
    }

    // GET api/departments/{id} - Get a single department by ID
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound(new { message = $"Department with ID {id} was not found." });
        }

        return Ok(new DepartmentResponseDto
        {
            Id = department.Id,
            Code = department.Code,
            Name = department.Name,
            Description = department.Description
        });
    }
}
