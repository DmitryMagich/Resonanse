using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/tracks")]
public class TracksController : ControllerBase
{
    private readonly ILibraryQueryService _query;
    private readonly ILibraryEditService _edit;

    public TracksController(ILibraryQueryService query, ILibraryEditService edit)
    {
        _query = query;
        _edit = edit;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TrackDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] TrackSortField sortBy = TrackSortField.Title,
        [FromQuery] SortDirection sortDir = SortDirection.Asc,
        [FromQuery] Guid? artistId = null,
        [FromQuery] Guid? albumId = null,
        [FromQuery] string[]? formats = null,
        [FromQuery] int? minBitDepth = null,
        [FromQuery] int? minSampleRate = null,
        [FromQuery] bool? hasCover = null,
        CancellationToken ct = default)
    {
        var query = new TrackQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDir = sortDir,
            ArtistId = artistId,
            AlbumId = albumId,
            Formats = formats?.ToList(),
            MinBitDepth = minBitDepth,
            MinSampleRate = minSampleRate,
            HasCover = hasCover
        };

        return Ok(await _query.GetTracksAsync(query, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrackDetailsDto>> GetById(Guid id, CancellationToken ct)
    {
        var track = await _query.GetTrackAsync(id, ct);
        return track is null ? NotFound() : Ok(track);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TrackDto>> Patch(Guid id, [FromBody] UpdateTrackRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _edit.UpdateTrackAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}