using System.Net.Http.Json;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiCurrentUserService(ApiClient apiClient) : ICurrentUserService
{
    public async Task<CurrentUser> GetAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/profile/me");
        using var response = await apiClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<ApiProfileResponse>()
            ?? throw new InvalidOperationException("The API returned an empty profile.");

        return new CurrentUser(profile.FullName);
    }
}