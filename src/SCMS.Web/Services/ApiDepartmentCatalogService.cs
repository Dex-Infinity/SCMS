using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiDepartmentCatalogService(IHttpClientFactory clients, IMemoryCache cache) : IDepartmentCatalogService
{
    private static readonly List<DepartmentOption> FallbackDepartments = new()
    {
        new DepartmentOption { Id = 1, Code = "CS", Name = "Computer Science" },
        new DepartmentOption { Id = 2, Code = "EE", Name = "Electrical Engineering" },
        new DepartmentOption { Id = 3, Code = "AA", Name = "Academic Affairs" }
    };

    public async Task<IReadOnlyList<DepartmentOption>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await cache.GetOrCreateAsync("departments:all", async entry =>
        {
            var client = clients.CreateClient("SCMS.Api.Public");
            const int maxAttempts = 2;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(5));

                    var result = await client.GetFromJsonAsync<List<DepartmentOption>>("api/departments", cts.Token);
                    if (result is { Count: > 0 })
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                        return result;
                    }
                }
                catch
                {
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    }
                }
            }

            // If backend is sleeping/warming up, provide fallback list and re-check sooner
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return FallbackDepartments;
        });

        return departments ?? FallbackDepartments;
    }
}