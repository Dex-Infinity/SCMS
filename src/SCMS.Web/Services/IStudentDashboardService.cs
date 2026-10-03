using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>
/// Supplies the student dashboard. Implementations should throw on failure so the page can show an error.
/// Real implementation: GET api/complaints/student/{studentId}, then count by status and take the latest few.
/// </summary>
public interface IStudentDashboardService
{
    Task<StudentDashboardData> GetAsync(CancellationToken cancellationToken = default);
}
