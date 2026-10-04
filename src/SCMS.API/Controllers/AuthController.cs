using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCMS.API.DTOs;
using SCMS.API.Services;

namespace SCMS.API.Controllers;

// Controller for user registration and authentication
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // Inject auth service
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register - Registration is temporarily unavailable.
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public IActionResult Register()
    {
        return StatusCode(StatusCodes.Status410Gone, new { message = "Registration is temporarily unavailable." });
    }

    // POST api/auth/login - Authenticate a user and return a JWT token
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(result);
    }
}
