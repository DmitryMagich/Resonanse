using Microsoft.AspNetCore.Mvc;
using Resonanse.Api.Startup;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly ILibraryScanner _scanner;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LibraryController> _logger;

    public LibraryController(
        ILibraryScanner scanner,
        IConfiguration configuration,
        ILogger<LibraryController> logger)
    {
        _scanner = scanner;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Сканирует папку с музыкой.
    /// Если path не указан - берётся Resonanse:LibraryPath или ~/Music.
    /// </summary>
    [HttpPost("scan")]
    public async Task<ActionResult<ScanResult>> Scan(
        [FromQuery] string? path,
        CancellationToken ct)
    {
        var scanPath = path;
        if (string.IsNullOrWhiteSpace(scanPath))
            scanPath = _configuration["Resonanse:LibraryPath"];
        if (string.IsNullOrWhiteSpace(scanPath))
            scanPath = DefaultPaths.GetMusicFolder();

        if (!Directory.Exists(scanPath))
            return BadRequest(new { error = $"Directory not found: {scanPath}" });

        _logger.LogInformation("Starting scan of {Path}", scanPath);

        var result = await _scanner.ScanAsync(scanPath, ct);

        _logger.LogInformation(
            "Scan done. Processed: {Processed}, Created: {Created}, Errors: {Errors}",
            result.FilesProcessed, result.TracksCreated, result.Errors);

        return Ok(result);
    }
}