using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SCMS.API.Middleware;
using SCMS.API.Repositories;
using SCMS.API.Services;
using SCMS.Infrastructure.Data;
using SCMS.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

var deploymentOverrides = new Dictionary<string, string?>();
MapEnvironmentAlias("Jwt:Key", "SCMS_JWT_KEY");
MapEnvironmentAlias("Attachments:StoragePath", "SCMS_UPLOAD_PATH", "uploads");
MapEnvironmentAlias("Seed:AdminEmail", "SCMS_ADMIN_EMAIL", string.Empty);
MapEnvironmentAlias("Seed:AdminPassword", "SCMS_ADMIN_PASSWORD", string.Empty);
MapEnvironmentAlias("Seed:AdminUserName", "SCMS_ADMIN_USERNAME", string.Empty);
MapEnvironmentAlias("Seed:AdminFullName", "SCMS_ADMIN_FULLNAME", string.Empty);
builder.Configuration.AddInMemoryCollection(deploymentOverrides);

void MapEnvironmentAlias(string configurationKey, string environmentKey, string? defaultValue = null)
{
    var configuredValue = builder.Configuration[configurationKey];
    if (string.IsNullOrWhiteSpace(configuredValue) || configuredValue.StartsWith("#{", StringComparison.Ordinal))
    {
        var envValue = builder.Configuration[environmentKey];
        if (!string.IsNullOrWhiteSpace(envValue) && !envValue.StartsWith("#{", StringComparison.Ordinal))
        {
            deploymentOverrides[configurationKey] = envValue;
        }
        else if (defaultValue != null)
        {
            deploymentOverrides[configurationKey] = defaultValue;
        }
    }
}

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path,
                Extensions = { ["traceId"] = context.HttpContext.TraceIdentifier }
            };

            return new BadRequestObjectResult(problemDetails);
        };
    });

// Configure CORS for web frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var configuredOrigins = builder.Configuration["Cors:AllowedOrigins"]
            ?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin)) return false;
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    if (uri.Host == "localhost" || uri.Host == "127.0.0.1" || uri.Host.EndsWith(".onrender.com", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return configuredOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Complaint Management System (SCMS) API",
        Version = "v1",
        Description = "Backend REST API endpoints for SCMS complaint submission, status tracking, assignments, and notifications."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer' followed by a space and your JWT token.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// Prefer .NET's connection-string setting, then Render's PostgreSQL URL.
var connectionString = builder.Configuration["DATABASE_URL"];
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.StartsWith("#{", StringComparison.Ordinal))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.StartsWith("#{", StringComparison.Ordinal))
{
    connectionString = builder.Configuration["SCMS_DB_CONNECTION_STRING"];
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection or DATABASE_URL with a PostgreSQL connection string before starting the API.");
}

connectionString = NormalizePostgresConnectionString(connectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

static string NormalizePostgresConnectionString(string value)
{
    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
        || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        if (!value.Contains("Trust Server Certificate", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var rawBuilder = new Npgsql.NpgsqlConnectionStringBuilder(value)
                {
                    TrustServerCertificate = true
                };
                return rawBuilder.ConnectionString;
            }
            catch
            {
                return value;
            }
        }
        return value;
    }

    var credentials = uri.UserInfo.Split(':', 2);
    var database = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
    if (credentials.Length != 2 || string.IsNullOrWhiteSpace(database))
    {
        throw new InvalidOperationException("DATABASE_URL must include a PostgreSQL username, password, host, and database name.");
    }

    var postgresConnection = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = database,
        Username = Uri.UnescapeDataString(credentials[0]),
        Password = Uri.UnescapeDataString(credentials[1]),
        SslMode = Npgsql.SslMode.Require,
        TrustServerCertificate = true
    };

    var sslMode = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(option => option.Split('=', 2))
        .FirstOrDefault(parts => parts.Length == 2 && parts[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase));
    if (sslMode is { Length: 2 } && Enum.TryParse<Npgsql.SslMode>(Uri.UnescapeDataString(sslMode[1]), true, out var parsedSslMode))
    {
        postgresConnection.SslMode = parsedSslMode;
    }

    return postgresConnection.ConnectionString;
}

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Bearer authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddOptions<JwtSettings>()
    .Bind(jwtSection)
    .Validate(settings => System.Text.Encoding.UTF8.GetByteCount(settings.Key) >= 32
        && !string.IsNullOrWhiteSpace(settings.Issuer)
        && !string.IsNullOrWhiteSpace(settings.Audience),
        "Configure a JWT key of at least 32 bytes, issuer, and audience.")
    .ValidateOnStart();

var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Dependency Injection Registrations
builder.Services.Configure<AttachmentOptions>(builder.Configuration.GetSection("Attachments"));
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection("Seed"));

var app = builder.Build();

// Catch unhandled exceptions and return consistent ProblemDetails responses
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable CORS for frontend integration
app.UseCors("AllowFrontend");

// Apply database migrations and seed configured roles/admin before serving requests.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var initializer = services.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
