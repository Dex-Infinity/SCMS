using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCMS.API.DTOs;
using SCMS.API.Services;

namespace SCMS.API.Controllers;

// Controller for the current user to read and save preferences
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    // GET api/settings/me - Get the current user's preferences (defaults when none saved)
    [HttpGet("me")]
    [ProducesResponseType(typeof(SettingsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Unable to identify the current user." });

        var result = await _settingsService.GetSettingsAsync(userId);
        return Ok(result);
    }

    // PUT api/settings/me - Save (create or update) the current user's preferences
    [HttpPut("me")]
    [ProducesResponseType(typeof(SettingsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveMe([FromBody] SettingsUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Unable to identify the current user." });

        var saved = await _settingsService.SaveSettingsAsync(userId, dto);
        return Ok(saved);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}
