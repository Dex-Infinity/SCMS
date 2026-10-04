using System.Net.Http.Json;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiDepartmentCatalogService(IHttpClientFactory clients) : IDepartmentCatalogService
{
    public async Task<IReadOnlyList<DepartmentOption>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await clients.CreateClient("SCMS.Api.Public")
            .GetFromJsonAsync<List<DepartmentOption>>("api/departments", cancellationToken)
        ?? throw new HttpRequestException("The API returned an empty department list.");
}