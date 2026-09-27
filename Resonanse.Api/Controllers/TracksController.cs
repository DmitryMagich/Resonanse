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
        CancellationToken ct = default)
    {
        return Ok(await _query.GetTracksAsync(page, pageSize, search, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrackDetailsDto>> GetById(Guid id, CancellationToken ct)
    {
        var track = await _query.GetTrackAsync(id, ct);
        return track is null ? NotFound() : Ok(track);
    }

    /// <summary>
    /// Редактирование метаданных трека. Поля, которые не переданы (или null) - не меняются.
    /// </summary>
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