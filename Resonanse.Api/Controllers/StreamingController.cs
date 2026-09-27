using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/stream")]
public class StreamController : ControllerBase
{
    private readonly IStreamService _streamService;

    public StreamController(IStreamService streamService)
    {
        _streamService = streamService;
    }

    /// <summary>
    /// Стриминг конкретного файла (TrackFile) с поддержкой Range Requests.
    /// </summary>
    [HttpGet("{trackFileId:guid}")]
    public async Task<IActionResult> StreamFile(Guid trackFileId, CancellationToken ct)
    {
        var info = await _streamService.GetStreamInfoAsync(trackFileId, ct);
        if (info is null) return NotFound();

        return PhysicalFile(
            physicalPath: info.FilePath,
            contentType: info.ContentType,
            fileDownloadName: null,
            enableRangeProcessing: true);
    }

    /// <summary>
    /// Стриминг лучшей версии трека (FLAC > ALAC > WAV > ... > MP3).
    /// Можно указать ?format=mp3 для выбора конкретного.
    /// </summary>
    [HttpGet("track/{trackId:guid}")]
    public async Task<IActionResult> StreamTrack(
        Guid trackId,
        [FromQuery] string? format,
        CancellationToken ct)
    {
        var info = await _streamService.GetBestStreamForTrackAsync(trackId, format, ct);
        if (info is null) return NotFound();

        return PhysicalFile(
            physicalPath: info.FilePath,
            contentType: info.ContentType,
            fileDownloadName: null,
            enableRangeProcessing: true);
    }
}