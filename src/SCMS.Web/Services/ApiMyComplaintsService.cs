using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiMyComplaintsService(
    ApiClient apiClient,
    AuthenticationStateProvider authenticationStateProvider) : IMyComplaintsService
{
    public async Task<IReadOnlyList<ComplaintSummary>> GetAsync(CancellationToken cancellationToken = default)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!int.TryParse(state.User.FindFirst("student_id")?.Value, out var studentId))
        {
            throw new InvalidOperationException("This account is not linked to a student profile.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/complaints/student/{studentId}");
        using var response = await apiClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var complaints = await response.Content.ReadFromJsonAsync<List<ApiComplaintResponse>>(cancellationToken: cancellationToken) ?? [];

        return complaints.Select(MapComplaint).ToList();
    }

    public async Task<ComplaintSummary?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/complaints/{id}");
        using var response = await apiClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        var complaint = await response.Content.ReadFromJsonAsync<ApiComplaintResponse>(cancellationToken: cancellationToken);
        return complaint is null ? null : MapComplaint(complaint);
    }

    private static ComplaintSummary MapComplaint(ApiComplaintResponse complaint)
    {
        var status = Enum.TryParse<ComplaintStatus>(complaint.StatusText, ignoreCase: true, out var parsedStatus)
            ? parsedStatus
            : throw new InvalidOperationException($"The API returned an unknown complaint status '{complaint.StatusText}'.");
        var reference = $"CMP-{complaint.CreatedAt.Year}-{complaint.Id:D3}";
        var category = complaint.DepartmentName ?? "Unassigned";
        return new ComplaintSummary(complaint.Id, reference, complaint.Title, category, complaint.CreatedAt, status, complaint.Description);
    }
}