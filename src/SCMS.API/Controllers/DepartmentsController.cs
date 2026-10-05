using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Controllers;

// Controller for retrieving academic and administrative departments
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[AllowAnonymous]
public class DepartmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache? _cache;

    public DepartmentsController(ApplicationDbContext context, IMemoryCache? cache = null)
    {
        _context = context;
        _cache = cache;
    }

    // GET api/departments - Get list of all departments
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DepartmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        if (_cache is null)
        {
            return Ok(await QueryDepartmentsAsync(cancellationToken));
        }

        var departments = await _cache.GetOrCreateAsync("departments:all", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await QueryDepartmentsAsync(cancellationToken);
        });

        return Ok(departments ?? []);
    }

    private Task<List<DepartmentResponseDto>> QueryDepartmentsAsync(CancellationToken cancellationToken) =>
        _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                Description = d.Description
            })
            .ToListAsync(cancellationToken);

    // GET api/departments/{id} - Get a single department by ID
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

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
