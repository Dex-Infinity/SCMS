using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SCMS.Infrastructure.Identity;

namespace SCMS.API.Services;

public class SeedOptions
{
    public bool SeedOnStartup { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminUserName { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = string.Empty;
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
        await SeedRolesAsync();
        if (!_seedOptions.SeedOnStartup)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_seedOptions.AdminEmail)
            || string.IsNullOrWhiteSpace(_seedOptions.AdminPassword)
            || string.IsNullOrWhiteSpace(_seedOptions.AdminUserName)
            || string.IsNullOrWhiteSpace(_seedOptions.AdminFullName))
        {
            throw new InvalidOperationException("Admin seed details must be configured when Seed:SeedOnStartup is enabled.");
        }

        await SeedAdminUserAsync();
        await SeedStudentUsersAsync();
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

    private async Task SeedStudentUsersAsync()
    {
        var sampleStudents = new[]
        {
            ("collins@student.university.edu", "collins", "Collins Edumadze", "Student@123456"),
            ("jessica@student.university.edu", "jessica", "Jessica Puozaa", "Student@123456")
        };

        foreach (var (email, userName, fullName, password) in sampleStudents)
        {
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null) continue;

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                FullName = fullName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, StudentRole);
            }
        }
    }
}
