using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Resonanse.Api.Startup;
using Resonanse.Application;
using Resonanse.Infrastructure;
using Resonanse.Infrastructure.Auth;

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
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || jwtOptions.SigningKey.Length < 32)
    throw new InvalidOperationException(
        "Resonanse:Jwt:SigningKey must be set in configuration (min 32 characters).");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
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