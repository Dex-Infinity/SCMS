using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.Infrastructure.Data;
using SCMS.Infrastructure.Identity;

namespace SCMS.API.Services;

public interface IProfileService
{
    Task<ProfileResponseDto?> GetProfileAsync(string userId);
    Task<ProfileResponseDto?> UpdateProfileAsync(string userId, ProfileUpdateDto dto);
}

// Service for reading and updating the current user's personal details
public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ProfileService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // Fetch the current user's profile
    public async Task<ProfileResponseDto?> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return MapToResponseDto(user, roles);
    }

    // Update personal details (full name, username, phone number)
    public async Task<ProfileResponseDto?> UpdateProfileAsync(string userId, ProfileUpdateDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        if (!string.Equals(user.UserName, dto.UserName, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new InvalidOperationException("This username is already taken.");
            }
        }

        user.FullName = dto.FullName;
        user.UserName = dto.UserName;
        user.PhoneNumber = dto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Profile update failed: {errors}");
        }

        // Synchronize updated FullName with Student table if user is a student
        if (!string.IsNullOrEmpty(user.Email))
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == user.Email);
            if (student != null)
            {
                student.FullName = dto.FullName;
                await _context.SaveChangesAsync();
            }
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return MapToResponseDto(user, roles);
    }

    private static ProfileResponseDto MapToResponseDto(ApplicationUser user, List<string> roles)
    {
        return new ProfileResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };
    }
}
