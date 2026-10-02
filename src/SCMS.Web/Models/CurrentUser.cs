namespace SCMS.Web.Models;

public sealed record CurrentUser(string DisplayName)
{
    public string FirstName =>
        DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? DisplayName;
}
