using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.Domain.Entities;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Services;

public interface ISettingsService
{
    Task<SettingsResponseDto> GetSettingsAsync(string userId);
    Task<SettingsResponseDto> SaveSettingsAsync(string userId, SettingsUpdateDto dto);
}

// Service for reading and saving per-user preferences (upsert semantics)
public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _dbContext;

    public SettingsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Fetch preferences, returning defaults when none saved yet
    public async Task<SettingsResponseDto> GetSettingsAsync(string userId)
    {
        var settings = await _dbContext.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null) return DefaultSettings(userId);

        return MapToResponseDto(settings);
    }

    // Create or update the user's preferences
    public async Task<SettingsResponseDto> SaveSettingsAsync(string userId, SettingsUpdateDto dto)
    {
        var settings = await _dbContext.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new UserSetting { UserId = userId };
            _dbContext.UserSettings.Add(settings);
        }

        settings.EmailNotifications = dto.EmailNotifications;
        settings.SmsNotifications = dto.SmsNotifications;
        settings.Theme = dto.Theme;
        settings.Language = dto.Language;
        settings.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return MapToResponseDto(settings);
    }

    private static SettingsResponseDto DefaultSettings(string userId)
    {
        return new SettingsResponseDto
        {
            UserId = userId,
            EmailNotifications = true,
            SmsNotifications = false,
            Theme = "light",
            Language = "en",
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static SettingsResponseDto MapToResponseDto(UserSetting settings)
    {
        return new SettingsResponseDto
        {
            UserId = settings.UserId,
            EmailNotifications = settings.EmailNotifications,
            SmsNotifications = settings.SmsNotifications,
            Theme = settings.Theme,
            Language = settings.Language,
            UpdatedAt = settings.UpdatedAt
        };
    }
}
