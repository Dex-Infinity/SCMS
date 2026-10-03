using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCMS.API.DTOs;
using SCMS.API.Services;

namespace SCMS.API.Controllers;

// Controller for the current user to view and update personal details
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    // GET api/profile/me - Get the current user's profile
    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Unable to identify the current user." });

        var result = await _profileService.GetProfileAsync(userId);
        if (result == null) return NotFound(new { message = "User profile was not found." });

        return Ok(result);
    }

    // PUT api/profile/me - Update personal details (full name, username, phone number)
    [HttpPut("me")]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe([FromBody] ProfileUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Unable to identify the current user." });

        try
        {
            var updated = await _profileService.UpdateProfileAsync(userId, dto);
            if (updated == null) return NotFound(new { message = "User profile was not found." });

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}
