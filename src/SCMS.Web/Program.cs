using SCMS.Web.Components;
using SCMS.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Json;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5000/")
});
builder.Services.AddScoped<ScmsApiClient>();

// TODO: swap for the real API-backed implementation once auth + endpoints are wired up.
builder.Services.AddScoped<IComplaintSubmissionService, MockComplaintSubmissionService>();
builder.Services.AddScoped<IStudentDashboardService, MockStudentDashboardService>();
builder.Services.AddScoped<IMyComplaintsService, MockMyComplaintsService>();
builder.Services.AddScoped<ICurrentUserService, MockCurrentUserService>();
builder.Services.AddScoped<IAnalyticsService, MockAnalyticsService>();

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

    var result = await response.Content.ReadFromJsonAsync<SCMS.Web.Models.LoginResponse>();
    if (result is null || string.IsNullOrWhiteSpace(result.Token))
    {
        return Results.Redirect("/login?error=1");
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, result.FullName),
        new(ClaimTypes.Email, result.Email),
        new("access_token", result.Token)
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

app.MapPost("/account/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
