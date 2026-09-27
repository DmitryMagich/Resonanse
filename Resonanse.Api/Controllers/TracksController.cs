using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/tracks")]
public class TracksController : ControllerBase
{
    private readonly ILibraryQueryService _query;

    public TracksController(ILibraryQueryService query)
    {
        _query = query;
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
}