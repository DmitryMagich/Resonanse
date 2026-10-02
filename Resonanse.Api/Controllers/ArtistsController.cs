using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/artists")]
public class ArtistsController : ControllerBase
{
    private readonly ILibraryQueryService _query;
    private readonly ILibraryEditService _edit;

    public ArtistsController(ILibraryQueryService query, ILibraryEditService edit)
    {
        _query = query;
        _edit = edit;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ArtistDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] ArtistSortField sortBy = ArtistSortField.Name,
        [FromQuery] SortDirection sortDir = SortDirection.Asc,
        CancellationToken ct = default)
    {
        var query = new ArtistQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDir = sortDir
        };

        return Ok(await _query.GetArtistsAsync(query, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArtistDetailsDto>> GetById(Guid id, CancellationToken ct)
    {
        var artist = await _query.GetArtistAsync(id, ct);
        return artist is null ? NotFound() : Ok(artist);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ArtistDto>> Patch(Guid id, [FromBody] UpdateArtistRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _edit.UpdateArtistAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}