namespace SCMS.Web.Services;

public sealed class ApiSettings
{
    public const string SectionName = "ApiSettings";

    public string BaseUrl { get; set; } = "http://localhost:5000";
    public Dictionary<string, int> StudentIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> DepartmentIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public int RequireStudentId(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var match = StudentIds.FirstOrDefault(pair => string.Equals(pair.Key, email, StringComparison.OrdinalIgnoreCase));
            if (match.Value > 0)
            {
                return match.Value;
            }
        }

        throw new InvalidOperationException($"Configure ApiSettings:StudentIds with the numeric student ID for '{email ?? "the signed-in user"}'.");
    }

    public int RequireDepartmentId(string category) => DepartmentIds.TryGetValue(category, out var departmentId) && departmentId > 0
        ? departmentId
        : throw new InvalidOperationException($"Configure ApiSettings:DepartmentIds:{category} with the matching SCMS.API department ID.");
}

internal sealed record ApiLoginRequest(string Email, string Password);
internal sealed record ApiRegisterRequest(string Email, string UserName, string FullName, string Password);

internal sealed class ApiAuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}