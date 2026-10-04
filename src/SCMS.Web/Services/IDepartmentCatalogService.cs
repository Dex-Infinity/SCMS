using SCMS.Web.Models;

namespace SCMS.Web.Services;

public interface IDepartmentCatalogService
{
    Task<IReadOnlyList<DepartmentOption>> GetAllAsync(CancellationToken cancellationToken = default);
}