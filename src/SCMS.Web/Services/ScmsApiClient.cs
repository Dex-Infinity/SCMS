using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SCMS.Web.Models;

namespace SCMS.Web.Services;

public sealed class ScmsApiClient(HttpClient http, AuthenticationStateProvider authenticationState)
{
    public Task<IReadOnlyList<ApiComplaint>> GetComplaintsAsync(int? departmentId = null, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ApiComplaint>>(HttpMethod.Get,
            departmentId.HasValue ? $"api/complaints/department/{departmentId.Value}" : "api/complaints",
            cancellationToken: cancellationToken);

    public Task<ApiComplaint> GetComplaintAsync(int id, CancellationToken cancellationToken = default) =>
        SendAsync<ApiComplaint>(HttpMethod.Get, $"api/complaints/{id}", cancellationToken: cancellationToken);

    public Task<ApiComplaint> UpdateComplaintStatusAsync(int id, ComplaintStatus status, CancellationToken cancellationToken = default) =>
        SendAsync<ApiComplaint>(HttpMethod.Put, $"api/complaints/{id}/status", new ComplaintStatusUpdate { Status = status }, cancellationToken);

    public Task<ApiComplaint> AssignComplaintAsync(int id, ComplaintAssignment assignment, CancellationToken cancellationToken = default) =>
        SendAsync<ApiComplaint>(HttpMethod.Put, $"api/complaints/{id}/assign", assignment, cancellationToken);

    public Task<IReadOnlyList<ApiNotification>> GetNotificationsAsync(bool? unreadOnly = null, CancellationToken cancellationToken = default)
    {
        var path = "api/notifications";
        if (unreadOnly.HasValue) path += $"?unreadOnly={unreadOnly.Value.ToString().ToLowerInvariant()}";
        return SendAsync<IReadOnlyList<ApiNotification>>(HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    public Task<ApiNotification> GetNotificationAsync(int id, CancellationToken cancellationToken = default) =>
        SendAsync<ApiNotification>(HttpMethod.Get, $"api/notifications/{id}", cancellationToken: cancellationToken);

    public Task<IReadOnlyList<ApiNotification>> GetNotificationsForUserAsync(string userId, bool? unreadOnly = null, CancellationToken cancellationToken = default)
    {
        var path = $"api/notifications/user/{Uri.EscapeDataString(userId)}";
        if (unreadOnly.HasValue) path += $"?unreadOnly={unreadOnly.Value.ToString().ToLowerInvariant()}";
        return SendAsync<IReadOnlyList<ApiNotification>>(HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    public Task<ApiNotification> CreateNotificationAsync(ApiNotificationCreate notification, CancellationToken cancellationToken = default) =>
        SendAsync<ApiNotification>(HttpMethod.Post, "api/notifications", notification, cancellationToken);

    public Task DeleteNotificationAsync(int id, CancellationToken cancellationToken = default) =>
        SendAsync<object>(HttpMethod.Delete, $"api/notifications/{id}", cancellationToken: cancellationToken);

    public Task MarkNotificationReadAsync(int id, CancellationToken cancellationToken = default) =>
        SendAsync<object>(HttpMethod.Put, $"api/notifications/{id}/read", cancellationToken: cancellationToken);

    public Task MarkAllNotificationsReadAsync(CancellationToken cancellationToken = default) =>
        SendAsync<object>(HttpMethod.Put, "api/notifications/read-all", cancellationToken: cancellationToken);

    public Task<ApiProfile> GetProfileAsync(CancellationToken cancellationToken = default) =>
        SendAsync<ApiProfile>(HttpMethod.Get, "api/profile/me", cancellationToken: cancellationToken);

    public Task<ApiProfile> UpdateProfileAsync(ProfileUpdate profile, CancellationToken cancellationToken = default) =>
        SendAsync<ApiProfile>(HttpMethod.Put, "api/profile/me", profile, cancellationToken);

    public Task<ApiUserSettings> GetSettingsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<ApiUserSettings>(HttpMethod.Get, "api/settings/me", cancellationToken: cancellationToken);

    public Task<ApiUserSettings> UpdateSettingsAsync(SettingsUpdate settings, CancellationToken cancellationToken = default) =>
        SendAsync<ApiUserSettings>(HttpMethod.Put, "api/settings/me", settings, cancellationToken);

    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<UnreadCountResult>(HttpMethod.Get, "api/notifications/unread-count", cancellationToken: cancellationToken);
        return response.UnreadCount;
    }

    public Task<IReadOnlyList<ApiAttachment>> GetAttachmentsAsync(int complaintId, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ApiAttachment>>(HttpMethod.Get, $"api/attachments/complaints/{complaintId}", cancellationToken: cancellationToken);

    public Task DeleteAttachmentAsync(int id, CancellationToken cancellationToken = default) =>
        SendAsync<object>(HttpMethod.Delete, $"api/attachments/{id}", cancellationToken: cancellationToken);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken cancellationToken = default)
    {
        var principal = (await authenticationState.GetAuthenticationStateAsync()).User;
        var token = principal.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("Sign in to access this page.");
        }

        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        using var response = await http.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Your session has expired. Sign in again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(string.IsNullOrWhiteSpace(details) ? "The request could not be completed." : details,
                null, response.StatusCode);
        }

        if (typeof(T) == typeof(object))
        {
            return (T)(object)new object();
        }

        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        return result ?? throw new HttpRequestException("The server returned an empty response.");
    }

    private sealed class UnreadCountResult
    {
        public int UnreadCount { get; set; }
    }
}