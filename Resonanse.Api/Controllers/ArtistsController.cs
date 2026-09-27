using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/artists")]
public class ArtistsController : ControllerBase
{
    private readonly ILibraryQueryService _query;

    public ArtistsController(ILibraryQueryService query)
    {
        _query = query;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ArtistDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        return Ok(await _query.GetArtistsAsync(page, pageSize, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArtistDetailsDto>> GetById(Guid id, CancellationToken ct)
    {
        var artist = await _query.GetArtistAsync(id, ct);
        return artist is null ? NotFound() : Ok(artist);
    }
}