using Microsoft.EntityFrameworkCore;
using SCMS.API.Repositories;
using SCMS.API.Services;
using SCMS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Student Complaint Management System (SCMS) API",
        Version = "v1",
        Description = "Backend REST API endpoints for SCMS complaint submission, status tracking, assignments, and notifications."
    });
});

// Configure EF Core DbContext with SQL Server (and fallback to InMemory for seamless dev testing)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseInMemoryDatabase("SCMSDb_Dev");
    }
});

// Dependency Injection Registrations
builder.Services.Configure<AttachmentOptions>(builder.Configuration.GetSection("Attachments"));

// Dependency Injection Registrations
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();

var app = builder.Build();

// Ensure Database & Tables are created on app startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning("SQL Server LocalDB connection failed ({Message}). Falling back to In-Memory Database for API testing.", ex.Message);
    }
}

// Enable Swagger UI for development and testing
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SCMS API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
