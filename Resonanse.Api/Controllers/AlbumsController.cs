using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumsController : ControllerBase
{
    private readonly ILibraryQueryService _query;
    private readonly ILibraryEditService _edit;

    public AlbumsController(ILibraryQueryService query, ILibraryEditService edit)
    {
        _query = query;
        _edit = edit;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AlbumDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] AlbumSortField sortBy = AlbumSortField.Title,
        [FromQuery] SortDirection sortDir = SortDirection.Asc,
        [FromQuery] Guid? artistId = null,
        [FromQuery] int? yearFrom = null,
        [FromQuery] int? yearTo = null,
        [FromQuery] bool? hasCover = null,
        CancellationToken ct = default)
    {
        var query = new AlbumQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDir = sortDir,
            ArtistId = artistId,
            YearFrom = yearFrom,
            YearTo = yearTo,
            HasCover = hasCover
        };

        return Ok(await _query.GetAlbumsAsync(query, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AlbumDetailsDto>> GetById(Guid id, CancellationToken ct)
    {
        var album = await _query.GetAlbumAsync(id, ct);
        return album is null ? NotFound() : Ok(album);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<AlbumDto>> Patch(Guid id, [FromBody] UpdateAlbumRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _edit.UpdateAlbumAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}