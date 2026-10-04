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
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, StudentRole);

        // Ensure matching Student domain entity exists for complaints & foreign keys
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == registerDto.Email);
        if (student == null)
        {
            student = new Student
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                IndexNumber = !string.IsNullOrWhiteSpace(registerDto.IndexNumber) ? registerDto.IndexNumber : registerDto.UserName,
                DepartmentId = registerDto.DepartmentId ?? 1,
                CreatedAt = DateTime.UtcNow
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
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
            StudentId = student.Id,
            Roles = roles
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

        int? studentId = null;
        if (roles.Contains(StudentRole))
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == user.Email);
            if (student == null)
            {
                student = new Student
                {
                    FullName = user.FullName,
                    Email = user.Email!,
                    IndexNumber = user.UserName ?? user.Email!,
                    DepartmentId = 1,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }
            studentId = student.Id;
        }

        var token = await _tokenService.CreateTokenAsync(user, roles, studentId);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Email = user.Email!,
            UserName = user.UserName!,
            FullName = user.FullName,
            StudentId = studentId,
            Roles = roles
        };
    }
}
