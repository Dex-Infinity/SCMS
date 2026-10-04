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

builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5000/")
});
builder.Services.AddScoped<ScmsApiClient>();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
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

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<IComplaintSubmissionService, ApiComplaintSubmissionService>();
builder.Services.AddScoped<IStudentDashboardService, ApiStudentDashboardService>();
builder.Services.AddScoped<IMyComplaintsService, ApiMyComplaintsService>();
builder.Services.AddScoped<ICurrentUserService, ApiCurrentUserService>();
builder.Services.AddScoped<IAnalyticsService, ApiAnalyticsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

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
    var response = await clients.CreateClient("SCMS.Api.Public").PostAsJsonAsync("api/auth/login", login);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Redirect("/login?error=1");
    }

    var authenticated = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();
    if (authenticated is null || string.IsNullOrWhiteSpace(authenticated.Token))
    {
        return Results.Redirect("/login?error=1");
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, authenticated.FullName),
        new(ClaimTypes.Email, authenticated.Email),
        new("access_token", authenticated.Token),
        new(ApiClient.TokenClaim, authenticated.Token)
    };
    claims.AddRange(authenticated.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        new AuthenticationProperties { ExpiresUtc = authenticated.ExpiresAt });

    var destination = Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//")
        ? returnUrl
        : authenticated.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ? AppRoutes.AdminDashboard : AppRoutes.Dashboard;
    return Results.Redirect(destination);
});

// Also support legacy/alternative /account/login endpoint
app.MapPost("/account/login", async (HttpContext context, IHttpClientFactory clientFactory, IConfiguration configuration) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        return Results.Redirect("/login?error=1");
    }

    var api = clientFactory.CreateClient();
    api.BaseAddress = new Uri(configuration["Api:BaseUrl"] ?? "http://localhost:5000/");
    using var response = await api.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
    if (!response.IsSuccessStatusCode)
    {
        return Results.Redirect("/login?error=1");
    }

    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
    if (result is null || string.IsNullOrWhiteSpace(result.Token))
    {
        return Results.Redirect("/login?error=1");
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, result.FullName),
        new(ClaimTypes.Email, result.Email),
        new("access_token", result.Token),
        new(ApiClient.TokenClaim, result.Token)
    };
    claims.AddRange(result.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
        new AuthenticationProperties { ExpiresUtc = result.ExpiresAt });

    var destination = Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//")
        ? returnUrl
        : result.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ? AppRoutes.AdminDashboard : AppRoutes.Dashboard;
    return Results.Redirect(destination);
});

app.MapPost("/auth/register", async (HttpContext context, IHttpClientFactory clients) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["Email"].ToString();
    var registration = new ApiRegisterRequest(
        email,
        email,
        form["FullName"].ToString(),
        form["Password"].ToString());
    var response = await clients.CreateClient("SCMS.Api.Public").PostAsJsonAsync("api/auth/register", registration);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Redirect("/signup?error=1");
    }

    var authenticated = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();
    if (authenticated is null || string.IsNullOrWhiteSpace(authenticated.Token))
    {
        return Results.Redirect("/signup?error=1");
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, authenticated.FullName),
        new(ClaimTypes.Email, authenticated.Email),
        new("access_token", authenticated.Token),
        new(ApiClient.TokenClaim, authenticated.Token)
    };
    claims.AddRange(authenticated.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        new AuthenticationProperties { ExpiresUtc = authenticated.ExpiresAt });

    return Results.Redirect(AppRoutes.Dashboard);
});

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapPost("/account/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
