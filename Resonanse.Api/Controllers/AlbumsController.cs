using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[Authorize(Roles = "Admin,User")]
[ApiController]
[Route("api/albums")]
public class AlbumsController : ControllerBase
{
    private readonly ILibraryQueryService _query;
    private readonly ILibraryEditService _edit;
    private readonly ICoverStorage _coverStorage;

    public AlbumsController(
        ILibraryQueryService query,
        ILibraryEditService edit,
        ICoverStorage coverStorage)
    {
        _query = query;
        _edit = edit;
        _coverStorage = coverStorage;
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

    [AllowAnonymous]
    [HttpGet("{id:guid}/cover")]
    public async Task<IActionResult> GetCover(Guid id, CancellationToken ct)
    {
        var album = await _query.GetAlbumAsync(id, ct);
        if (album is null || string.IsNullOrWhiteSpace(album.CoverPath))
            return NotFound();

        var fullPath = _coverStorage.GetFullPath(album.CoverPath);
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

        Response.Headers.CacheControl = "public, max-age=86400";
        return PhysicalFile(fullPath, contentType);
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