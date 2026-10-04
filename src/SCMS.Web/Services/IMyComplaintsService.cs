using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>
/// All complaints filed by the signed-in student. The page filters, sorts and pages them in memory.
/// Real implementation: GET api/complaints/student/{studentId}. Throw on failure so the page can show an error.
/// </summary>
public interface IMyComplaintsService
{
    Task<IReadOnlyList<ComplaintSummary>> GetAsync(CancellationToken cancellationToken = default);
    Task<ComplaintSummary?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
