using Resonanse.Api.Startup;
using Resonanse.Application;
using Resonanse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Seed local peer
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
app.UseAuthorization();
app.MapControllers();

app.Run();