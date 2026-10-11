using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Services;
using Resonanse.Domain.Enums;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/stream")]
[AllowAnonymous]
public class StreamController : ControllerBase
{
    private readonly IStreamService _streamService;
    private readonly ICurrentUser _currentUser;

    public StreamController(IStreamService streamService, ICurrentUser currentUser)
    {
        _streamService = streamService;
        _currentUser = currentUser;
    }

    [HttpGet("{trackFileId:guid}")]
    public async Task<IActionResult> StreamFile(Guid trackFileId, CancellationToken ct)
    {
        var info = await _streamService.GetStreamInfoAsync(trackFileId, ct);
        if (info is null) return NotFound();

        if (!await CanAccessAsync(info.TrackId, ct))
            return Forbid();

        return PhysicalFile(
            physicalPath: info.FilePath,
            contentType: info.ContentType,
            fileDownloadName: null,
            enableRangeProcessing: true);
    }

    [HttpGet("track/{trackId:guid}")]
    public async Task<IActionResult> StreamTrack(
        Guid trackId,
        [FromQuery] string? format,
        CancellationToken ct)
    {
        var info = await _streamService.GetBestStreamForTrackAsync(trackId, format, ct);
        if (info is null) return NotFound();

        if (!await CanAccessAsync(trackId, ct))
            return Forbid();

        return PhysicalFile(
            physicalPath: info.FilePath,
            contentType: info.ContentType,
            fileDownloadName: null,
            enableRangeProcessing: true);
    }

    private async Task<bool> CanAccessAsync(Guid trackId, CancellationToken ct)
    {
        var role = _currentUser.Role;

        // Admin и User — доступ ко всему
        if (role is UserRole.Admin or UserRole.User)
            return true;

        // Guest и анонимы — только треки из тестового альбома
        return await _streamService.IsTrackInTestAlbumAsync(trackId, ct);
    }
}