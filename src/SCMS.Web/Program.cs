using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using SCMS.Web;
using SCMS.Web.Components;
using SCMS.Web.Models;
using SCMS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "SCMS.Web.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();

builder.Services.AddOptions<ApiSettings>()
    .Bind(builder.Configuration.GetSection(ApiSettings.SectionName))
    .Validate(settings => Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
        "Configure ApiSettings:BaseUrl with the absolute URL of the SCMS API.")
    .ValidateOnStart();
builder.Services.AddHttpClient("SCMS.Api", (services, client) =>
{
    var settings = services.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
});
builder.Services.AddHttpClient("SCMS.Api.Public", (services, client) =>
{
    var settings = services.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddScoped(services => new ScmsApiClient(
    services.GetRequiredService<IHttpClientFactory>().CreateClient("SCMS.Api"),
    services.GetRequiredService<AuthenticationStateProvider>()));
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<IComplaintSubmissionService, ApiComplaintSubmissionService>();
builder.Services.AddScoped<IStudentDashboardService, ApiStudentDashboardService>();
builder.Services.AddScoped<IMyComplaintsService, ApiMyComplaintsService>();
builder.Services.AddScoped<ICurrentUserService, ApiCurrentUserService>();
builder.Services.AddScoped<IAnalyticsService, ApiAnalyticsService>();
builder.Services.AddScoped<IDepartmentCatalogService, ApiDepartmentCatalogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Error", createScopeForErrors: true);
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/warmup", (IHttpClientFactory clients) =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            var client = clients.CreateClient("SCMS.Api.Public");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            await client.GetAsync("health", cts.Token);
        }
        catch { }
    });
    return Results.Ok(new { status = "warming" });
});

async Task<IResult> CompleteSignInAsync(HttpContext context, ApiAuthResponse authenticated, string? returnUrl)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, authenticated.FullName),
        new(ClaimTypes.Email, authenticated.Email),
        new("access_token", authenticated.Token),
        new(ApiClient.TokenClaim, authenticated.Token)
    };
    claims.AddRange(authenticated.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
    if (authenticated.StudentId.HasValue)
    {
        claims.Add(new Claim("student_id", authenticated.StudentId.Value.ToString()));
    }

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
        new AuthenticationProperties { ExpiresUtc = authenticated.ExpiresAt });

    var destination = Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) && returnUrl!.StartsWith('/') && !returnUrl.StartsWith("//")
        ? returnUrl
        : authenticated.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ? AppRoutes.AdminDashboard : AppRoutes.Dashboard;
    return Results.Redirect(destination);
}

async Task<IResult> DownloadAttachmentAsync(int id, HttpContext context, IHttpClientFactory clients)
{
    var token = context.User.FindFirstValue(ApiClient.TokenClaim);
    if (string.IsNullOrWhiteSpace(token)) return Results.Unauthorized();

    using var request = new HttpRequestMessage(HttpMethod.Get, $"api/attachments/{id}");
    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    using var response = await clients.CreateClient("SCMS.Api").SendAsync(request, context.RequestAborted);
    if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return Results.NotFound();
    if (!response.IsSuccessStatusCode) return Results.StatusCode((int)response.StatusCode);

    var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
        ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
        ?? $"attachment-{id}";
    fileName = Path.GetFileName(fileName);
    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
    var content = await response.Content.ReadAsByteArrayAsync(context.RequestAborted);
    return Results.File(content, contentType, fileName, enableRangeProcessing: true);
}

app.MapPost("/auth/login", async (HttpContext context, IHttpClientFactory clients) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["Email"].ToString();
    var password = form["Password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        return Results.Redirect("/login?error=1");
    }

    var login = new ApiLoginRequest(email, password);
    HttpResponseMessage response;
    try
    {
        response = await clients.CreateClient("SCMS.Api.Public").PostAsJsonAsync("api/auth/login", login);
    }
    catch (Exception)
    {
        return Results.Redirect("/login?error=warming");
    }

    if (!response.IsSuccessStatusCode)
    {
        return Results.Redirect("/login?error=1");
    }

    var authenticated = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();
    if (authenticated is null || string.IsNullOrWhiteSpace(authenticated.Token))
    {
        return Results.Redirect("/login?error=1");
    }

    return await CompleteSignInAsync(context, authenticated, returnUrl);
}).DisableAntiforgery();

app.MapPost("/auth/register", async (HttpContext context, IHttpClientFactory clients) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["Email"].ToString();
    if (!int.TryParse(form["DepartmentId"], out var departmentId))
    {
        return Results.Redirect("/signup?error=1&message=Select%20a%20valid%20department.");
    }

    var registration = new ApiRegisterRequest(
        email,
        email,
        form["FullName"].ToString(),
        form["Password"].ToString(),
        form["IndexNumber"].ToString(),
        departmentId);

    HttpResponseMessage response;
    try
    {
        response = await clients.CreateClient("SCMS.Api.Public").PostAsJsonAsync("api/auth/register", registration);
    }
    catch
    {
        return Results.Redirect("/signup?error=1&message=The%20registration%20service%20could%20not%20be%20reached.");
    }

    if (!response.IsSuccessStatusCode)
    {
        var errorMessage = "Registration failed. Check your information and try again.";
        try
        {
            var error = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            if (error.TryGetProperty("message", out var message) && message.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                errorMessage = message.GetString() ?? errorMessage;
            }
        }
        catch { }

        return Results.Redirect($"/signup?error=1&message={Uri.EscapeDataString(errorMessage)}");
    }

    var authenticated = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();
    if (authenticated is null || string.IsNullOrWhiteSpace(authenticated.Token))
    {
        return Results.Redirect("/signup?error=1&message=The%20API%20returned%20an%20invalid%20registration%20response.");
    }

    return await CompleteSignInAsync(context, authenticated, AppRoutes.Dashboard);
}).DisableAntiforgery();

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).DisableAntiforgery();

app.MapGet("/attachments/{id:int}/download", DownloadAttachmentAsync).RequireAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
