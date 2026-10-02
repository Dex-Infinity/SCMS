using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>Temporary stand-in until authentication is wired up.</summary>
public sealed class MockCurrentUserService : ICurrentUserService
{
    public Task<CurrentUser> GetAsync() => Task.FromResult(new CurrentUser("Alex Doe"));
}
