using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SCMS.Infrastructure.Identity;

namespace SCMS.API.Services;

public class SeedOptions
{
    public bool SeedOnStartup { get; set; } = true;
    public string AdminEmail { get; set; } = "admin@scms.com";
    public string AdminPassword { get; set; } = "Admin@123456";
    public string AdminUserName { get; set; } = "admin";
    public string AdminFullName { get; set; } = "System Administrator";
}

public interface IDbInitializer
{
    Task InitializeAsync();
}

public class DbInitializer : IDbInitializer
{
    public const string StudentRole = "Student";
    public const string AdminRole = "Admin";

    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SeedOptions _seedOptions;

    public DbInitializer(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<SeedOptions> seedOptions)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seedOptions = seedOptions.Value;
    }

    public async Task InitializeAsync()
    {
        if (!_seedOptions.SeedOnStartup)
        {
            return;
        }

        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in new[] { StudentRole, AdminRole })
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole(role));
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var admin = await _userManager.FindByEmailAsync(_seedOptions.AdminEmail);
        if (admin != null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = _seedOptions.AdminUserName,
            Email = _seedOptions.AdminEmail,
            FullName = _seedOptions.AdminFullName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, _seedOptions.AdminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, AdminRole);
        }
    }
}
