using Microsoft.AspNetCore.Components.Forms;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed record ComplaintSubmissionResult(int ComplaintId, string ReferenceNumber, string? AttachmentWarning = null);

/// <summary>
/// Sends a complaint (and its attachments) to the backend.
/// Implementations should throw if the submission fails so the page can show an error.
/// </summary>
public interface IComplaintSubmissionService
{
    Task<ComplaintSubmissionResult> SubmitAsync(
        ComplaintFormModel form,
        IReadOnlyList<IBrowserFile> attachments,
        CancellationToken cancellationToken = default);
}
