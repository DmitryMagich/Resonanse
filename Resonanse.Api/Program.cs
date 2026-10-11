using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Resonanse.Api.Middleware;
using Resonanse.Api.Startup;
using Resonanse.Application;
using Resonanse.Infrastructure;
using Resonanse.Infrastructure.Auth;
using Resonanse.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

// Controllers + глобальный [Authorize]
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

// Swagger + Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введи JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<Microsoft.Extensions.Options.IOptions<JwtOptions>>((bearer, jwt) =>
    {
        var o = jwt.Value;

        if (string.IsNullOrWhiteSpace(o.SigningKey) || o.SigningKey.Length < 32)
            throw new InvalidOperationException(
                "Resonanse:Jwt:SigningKey must be set in configuration (min 32 characters).");

        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = o.Issuer,
            ValidateAudience = true,
            ValidAudience = o.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// DI
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Error log channel + DbLogger
builder.Services.AddSingleton<ErrorLogChannel>();

var dbMinLevel = Enum.TryParse<LogLevel>(
    builder.Configuration["Logging:Db:MinLevel"], true, out var lvl)
    ? lvl
    : LogLevel.Warning;

var errorLogChannel = new ErrorLogChannel();
builder.Services.AddSingleton(errorLogChannel);

builder.Logging.AddProvider(new DbLoggerProvider(
    channel: errorLogChannel,
    minLevel: dbMinLevel,
    excludedPrefixes: new[]
    {
        "Microsoft.",
        "System.",
        "Resonanse.Infrastructure.Logging.ErrorLogWriterService",
        "Resonanse.Infrastructure.Logging.ErrorLogCleanupService"
    }));

// BackgroundServices
builder.Services.AddHostedService<ErrorLogWriterService>();

var retentionDays = builder.Configuration.GetValue<int?>("Logging:Db:RetentionDays") ?? 30;
builder.Services.AddSingleton<IHostedService>(sp =>
    new ErrorLogCleanupService(
        sp.GetRequiredService<IServiceScopeFactory>(),
        sp.GetRequiredService<ILogger<ErrorLogCleanupService>>(),
        retentionDays));

// AutoScan
builder.Services.Configure<AutoScanOptions>(
    builder.Configuration.GetSection(AutoScanOptions.SectionName));
builder.Services.AddHostedService<LibraryAutoScanService>();

var app = builder.Build();

// Миграции
if (builder.Configuration.GetValue<bool>("Resonanse:ApplyMigrationsOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Resonanse.Infrastructure.Persistence.ResonanseDbContext>();
    await db.Database.MigrateAsync();
}

// Seed local peer
var peerName = builder.Configuration["Resonanse:PeerName"];
if (string.IsNullOrWhiteSpace(peerName))
    peerName = DefaultPaths.GetDefaultPeerName();
await PeerSeeder.EnsureLocalPeerAsync(app.Services, peerName);

// Seed admin
var adminUsername = builder.Configuration["Resonanse:Admin:Username"];
var adminPassword = builder.Configuration["Resonanse:Admin:Password"];
if (!string.IsNullOrWhiteSpace(adminUsername) && !string.IsNullOrWhiteSpace(adminPassword))
{
    await UserSeeder.EnsureAdminAsync(app.Services, adminUsername, adminPassword);
}

// Exception logging
app.UseMiddleware<ExceptionLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }