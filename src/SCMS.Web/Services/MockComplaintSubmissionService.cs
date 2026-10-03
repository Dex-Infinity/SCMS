using Microsoft.AspNetCore.Components.Forms;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

/// <summary>
/// Temporary stand-in so the UI flow works before the API is wired up and auth exists.
/// Replace with an HttpClient-based implementation that calls:
///   POST api/complaints                                 (ComplaintCreateDto)
///   POST api/attachments/complaints/{complaintId}       (one multipart request per file)
/// and register it in Program.cs instead of this class.
/// </summary>
public sealed class MockComplaintSubmissionService : IComplaintSubmissionService
{
    private static int _lastId = 1000;

    public async Task<ComplaintSubmissionResult> SubmitAsync(
        ComplaintFormModel form,
        IReadOnlyList<IBrowserFile> attachments,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(700, cancellationToken);

        var id = Interlocked.Increment(ref _lastId);
        return new ComplaintSubmissionResult(id, $"CMP-{id:D6}");
    }
}
