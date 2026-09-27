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

    public LibraryController(ILibraryScanner scanner, IConfiguration configuration)
    {
        _scanner = scanner;
        _configuration = configuration;
    }

    [HttpPost("scan")]
    public async Task<ActionResult<ScanResult>> Scan(CancellationToken ct)
    {
        var path = _configuration["Resonanse:LibraryPath"];
        if (string.IsNullOrWhiteSpace(path))
            path = DefaultPaths.GetMusicFolder();

        var result = await _scanner.ScanAsync(path, ct);
        return Ok(result);
    }
}