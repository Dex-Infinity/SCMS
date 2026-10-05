using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiDepartmentCatalogService(IHttpClientFactory clients, IMemoryCache cache) : IDepartmentCatalogService
{
    public async Task<IReadOnlyList<DepartmentOption>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await cache.GetOrCreateAsync("departments:all", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
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
        });

        return departments ?? throw new HttpRequestException("The API returned an empty department list.");
    }
}