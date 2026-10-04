using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[AllowAnonymous]
public sealed class DepartmentsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DepartmentOptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var departments = await dbContext.Departments
            .AsNoTracking()
            .OrderBy(department => department.Name)
            .Select(department => new DepartmentOptionDto
            {
                Id = department.Id,
                Code = department.Code,
                Name = department.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(departments);
    }
}