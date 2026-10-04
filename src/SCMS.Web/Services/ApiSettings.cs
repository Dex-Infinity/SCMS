namespace SCMS.Web.Services;

public sealed class ApiSettings
{
    public const string SectionName = "ApiSettings";

       public string BaseUrl { get; set; } = string.Empty;
}

internal sealed record ApiLoginRequest(string Email, string Password);
internal sealed record ApiRegisterRequest(
    string Email,
    string UserName,
    string FullName,
    string Password,
    string IndexNumber,
    int DepartmentId);

internal sealed class ApiAuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public int? StudentId { get; set; }
}