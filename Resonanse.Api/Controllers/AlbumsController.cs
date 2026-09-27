using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumsController : ControllerBase
{
    private readonly ILibraryQueryService _query;

    public AlbumsController(ILibraryQueryService query)
    {
        _query = query;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AlbumDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] Guid? artistId = null,
        CancellationToken ct = default)
    {
        return Ok(await _query.GetAlbumsAsync(page, pageSize, artistId, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AlbumDetailsDto>> GetById(Guid id, CancellationToken ct)
    {
        var album = await _query.GetAlbumAsync(id, ct);
        return album is null ? NotFound() : Ok(album);
    }
}