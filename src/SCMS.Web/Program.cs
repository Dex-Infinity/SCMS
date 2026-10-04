using SCMS.Web.Components;
using SCMS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
