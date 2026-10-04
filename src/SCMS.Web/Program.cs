using System.Security.Claims;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using SCMS.Web;
using SCMS.Web.Components;
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
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = false;
    });
builder.Services.AddAuthorization();

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
    var login = new ApiLoginRequest(form["Email"].ToString(), form["Password"].ToString());
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
        new(ApiClient.TokenClaim, authenticated.Token)
    };
    claims.AddRange(authenticated.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        new AuthenticationProperties { ExpiresUtc = authenticated.ExpiresAt });

    return Results.Redirect(AppRoutes.Dashboard);
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
