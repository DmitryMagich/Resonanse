using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Resonanse.Api.Auth;
using Resonanse.Api.Startup;
using Resonanse.Application;
using Resonanse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Controllers + глобальный [Authorize] на все endpoint'ы
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

// Swagger + Basic Auth
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic авторизация. Введи username и password."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Basic"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Basic Auth
builder.Services.Configure<BasicAuthOptions>(
    builder.Configuration.GetSection(BasicAuthOptions.SectionName));

builder.Services
    .AddAuthentication(BasicAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>(
        BasicAuthHandler.SchemeName, null);

builder.Services.AddAuthorization();

// DI: Infrastructure + Application
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Seed local peer (создаётся один раз при старте)
var peerName = builder.Configuration["Resonanse:PeerName"];
if (string.IsNullOrWhiteSpace(peerName))
    peerName = DefaultPaths.GetDefaultPeerName();
await PeerSeeder.EnsureLocalPeerAsync(app.Services, peerName);

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