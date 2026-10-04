using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SCMS.API.DTOs;
using SCMS.Domain.Entities;
using SCMS.Infrastructure.Data;
using SCMS.Infrastructure.Identity;

namespace SCMS.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
}

public class AuthService : IAuthService
{
    public const string StudentRole = "Student";
    public const string AdminRole = "Admin";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _dbContext;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var department = await _dbContext.Departments.SingleOrDefaultAsync(item => item.Id == registerDto.DepartmentId);
        if (department == null)
        {
            throw new InvalidOperationException("Select a valid department.");
        }

        var student = await _dbContext.Students.SingleOrDefaultAsync(item => item.Email == registerDto.Email);
        if (student != null &&
            (!string.Equals(student.IndexNumber, registerDto.IndexNumber, StringComparison.OrdinalIgnoreCase) ||
             student.DepartmentId != registerDto.DepartmentId))
        {
            throw new InvalidOperationException("The student number or department does not match the student record.");
        }

        if (student == null && await _dbContext.Students.AnyAsync(item => item.IndexNumber == registerDto.IndexNumber))
        {
            throw new InvalidOperationException("A student with this student number already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, StudentRole);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            if (student == null)
            {
                student = new Student
                {
                    IndexNumber = registerDto.IndexNumber,
                    FullName = registerDto.FullName,
                    Email = registerDto.Email,
                    DepartmentId = registerDto.DepartmentId,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Students.Add(student);
                await _dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var roles = new List<string> { StudentRole };
        var token = await _tokenService.CreateTokenAsync(user, roles, student.Id);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Email = user.Email!,
            UserName = user.UserName!,
            FullName = user.FullName,
            Roles = roles,
            StudentId = student.Id
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return null;
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
        {
            return null;
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var studentId = await _dbContext.Students
            .Where(student => student.Email == user.Email)
            .Select(student => (int?)student.Id)
            .FirstOrDefaultAsync();
        var token = await _tokenService.CreateTokenAsync(user, roles, studentId);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Email = user.Email!,
            UserName = user.UserName!,
            FullName = user.FullName,
            Roles = roles,
            StudentId = studentId
        };
    }
}
