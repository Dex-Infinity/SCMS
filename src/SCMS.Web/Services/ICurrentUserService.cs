using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>Who is signed in. Back this with AuthenticationState once login is built.</summary>
public interface ICurrentUserService
{
    Task<CurrentUser> GetAsync();
}
