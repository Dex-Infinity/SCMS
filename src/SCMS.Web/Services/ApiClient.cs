using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;

namespace SCMS.Web.Services;

public sealed class ApiClient(IHttpClientFactory clients, AuthenticationStateProvider authenticationStateProvider)
{
    public const string TokenClaim = "scms:api-token";

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var authenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var token = authenticationState.User.FindFirst(TokenClaim)?.Value;
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await clients.CreateClient("SCMS.Api").SendAsync(request, cancellationToken);
    }
}