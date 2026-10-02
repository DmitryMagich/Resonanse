using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Resonanse.Api.Startup;
using Resonanse.Application;
using Resonanse.Infrastructure;
using Resonanse.Infrastructure.Auth;
using Resonanse.Infrastructure.Persistence;

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
        Description = "Введи JWT access token. Пример: <token> (без слова 'Bearer' — Swagger добавит сам)."
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

// JWT options + auth
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Конфигурируем JwtBearerOptions лениво через IOptions<JwtOptions>,
// чтобы ключ валидации брался из того же источника, что и в JwtTokenService.
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
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

// AutoScan
builder.Services.Configure<AutoScanOptions>(
    builder.Configuration.GetSection(AutoScanOptions.SectionName));
builder.Services.AddHostedService<LibraryAutoScanService>();

var app = builder.Build();

// Apply migrations
if (builder.Configuration.GetValue<bool>("Resonanse:ApplyMigrationsOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ResonanseDbContext>();
    await db.Database.MigrateAsync();
}

// Seed local peer
var peerName = builder.Configuration["Resonanse:PeerName"];
if (string.IsNullOrWhiteSpace(peerName))
    peerName = DefaultPaths.GetDefaultPeerName();
await PeerSeeder.EnsureLocalPeerAsync(app.Services, peerName);

// Seed admin (bootstrap) — только если нет ни одного user
var adminUsername = builder.Configuration["Resonanse:Admin:Username"];
var adminPassword = builder.Configuration["Resonanse:Admin:Password"];
if (!string.IsNullOrWhiteSpace(adminUsername) && !string.IsNullOrWhiteSpace(adminPassword))
{
    await UserSeeder.EnsureAdminAsync(app.Services, adminUsername, adminPassword);
}

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