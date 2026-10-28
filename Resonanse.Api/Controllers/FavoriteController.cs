using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/me/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favorites;

    public FavoritesController(IFavoriteService favorites)
    {
        _favorites = favorites;
    }

    [HttpPost("{trackId:guid}")]
    public async Task<IActionResult> Toggle(Guid trackId, CancellationToken ct)
    {
        var isFavorite = await _favorites.ToggleAsync(trackId, ct);
        return Ok(new { isFavorite });
    }
}