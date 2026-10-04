using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ApiMyComplaintsService(
    ApiClient apiClient,
    AuthenticationStateProvider authenticationStateProvider,
    IOptions<ApiSettings> settings) : IMyComplaintsService
{
    public async Task<IReadOnlyList<ComplaintSummary>> GetAsync(CancellationToken cancellationToken = default)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        var studentId = settings.Value.RequireStudentId(state.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/complaints/student/{studentId}");
        using var response = await apiClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var complaints = await response.Content.ReadFromJsonAsync<List<ApiComplaintResponse>>(cancellationToken: cancellationToken) ?? [];

        return complaints.Select(MapComplaint).ToList();
    }

    private ComplaintSummary MapComplaint(ApiComplaintResponse complaint)
    {
        var status = Enum.TryParse<ComplaintStatus>(complaint.StatusText, ignoreCase: true, out var parsedStatus)
            ? parsedStatus
            : throw new InvalidOperationException($"The API returned an unknown complaint status '{complaint.StatusText}'.");
        var department = complaint.DepartmentName;
        if (string.IsNullOrWhiteSpace(department) && complaint.DepartmentId is { } departmentId)
        {
            department = settings.Value.DepartmentIds.FirstOrDefault(pair => pair.Value == departmentId).Key;
        }

        var reference = $"CMP-{complaint.CreatedAt.Year}-{complaint.Id:D3}";
        var category = department is null ? "Unassigned" : ComplaintCategories.LabelFor(department);
        return new ComplaintSummary(complaint.Id, reference, complaint.Title, category, complaint.CreatedAt, status, complaint.Description);
    }
}