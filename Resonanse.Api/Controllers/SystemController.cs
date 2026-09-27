using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resonanse.Infrastructure.Persistence;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private readonly ResonanseDbContext _db;

    public SystemController(ResonanseDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Живой ли сервер + доступна ли БД. Без авторизации.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/health")]
    public async Task<IActionResult> Health(CancellationToken ct)
    {
        var dbOk = false;
        try
        {
            dbOk = await _db.Database.CanConnectAsync(ct);
        }
        catch
        {
            dbOk = false;
        }

        var status = dbOk ? "ok" : "degraded";
        var code = dbOk ? 200 : 503;

        return StatusCode(code, new
        {
            status,
            database = dbOk ? "connected" : "unreachable",
            timestamp = DateTime.UtcNow
        });
    }
    
    [HttpGet("version")]
    public async Task<IActionResult> Version(CancellationToken ct)
    {
        var trackCount = await _db.Tracks.CountAsync(ct);
        var fileCount = await _db.TrackFiles.CountAsync(ct);
        var peerCount = await _db.Peers.CountAsync(ct);

        var version = typeof(SystemController).Assembly
            .GetName().Version?.ToString() ?? "unknown";

        return Ok(new
        {
            name = "Resonanse",
            version,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            library = new
            {
                tracks = trackCount,
                files = fileCount,
                peers = peerCount
            }
        });
    }
}