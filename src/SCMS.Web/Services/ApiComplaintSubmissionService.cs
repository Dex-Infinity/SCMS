using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiComplaintSubmissionService(
    ApiClient apiClient,
    AuthenticationStateProvider authenticationStateProvider) : IComplaintSubmissionService
{
    private const long MaxAttachmentSize = 5 * 1024 * 1024;

    public async Task<ComplaintSubmissionResult> SubmitAsync(
        ComplaintFormModel form,
        IReadOnlyList<IBrowserFile> attachments,
        CancellationToken cancellationToken = default)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!int.TryParse(state.User.FindFirst("student_id")?.Value, out var studentId))
        {
            throw new UnauthorizedAccessException("This account is not linked to a student profile.");
        }

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "api/complaints")
        {
            Content = JsonContent.Create(new ApiComplaintCreateRequest
            {
                Title = form.Subject,
                Description = form.Description,
                StudentId = studentId,
                DepartmentId = form.DepartmentId
            })
        };
        using var createResponse = await apiClient.SendAsync(createRequest, cancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<ApiComplaintResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty complaint response.");
        var reference = $"CMP-{created.CreatedAt.Year}-{created.Id:D3}";
        var failedFiles = new List<string>();

        foreach (var file in attachments)
        {
            try
            {
                await UploadAttachmentAsync(created.Id, file, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidOperationException)
            {
                failedFiles.Add(file.Name);
            }
        }

        var warning = failedFiles.Count == 0
            ? null
            : $"Complaint {reference} was submitted, but these attachments could not be uploaded: {string.Join(", ", failedFiles)}. Contact support with your reference number.";

        return new ComplaintSubmissionResult(created.Id, reference, warning);
    }

    private async Task UploadAttachmentAsync(int complaintId, IBrowserFile file, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream(MaxAttachmentSize, cancellationToken);
        using var fileContent = new StreamContent(stream);
        if (MediaTypeHeaderValue.TryParse(file.ContentType, out var contentType))
        {
            fileContent.Headers.ContentType = contentType;
        }

        content.Add(fileContent, "file", file.Name);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/attachments/complaints/{complaintId}")
        {
            Content = content
        };
        using var response = await apiClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private sealed class ApiComplaintCreateRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int StudentId { get; init; }
        public int DepartmentId { get; init; }
    }
}