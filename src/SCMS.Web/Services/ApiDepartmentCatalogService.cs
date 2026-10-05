using System.Net.Http.Json;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiDepartmentCatalogService(IHttpClientFactory clients) : IDepartmentCatalogService
{
    public async Task<IReadOnlyList<DepartmentOption>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = clients.CreateClient("SCMS.Api.Public");
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await client.GetFromJsonAsync<List<DepartmentOption>>("api/departments", cancellationToken)
                    ?? throw new HttpRequestException("The API returned an empty department list.");
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        throw new HttpRequestException("The API could not be reached after multiple attempts.");
    }
}