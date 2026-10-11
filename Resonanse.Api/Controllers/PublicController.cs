using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Dtos;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController : ControllerBase
{
    private readonly IPublicAlbumService _albumService;
    private readonly IInviteRequestService _inviteRequestService;

    public PublicController(
        IPublicAlbumService albumService,
        IInviteRequestService inviteRequestService)
    {
        _albumService = albumService;
        _inviteRequestService = inviteRequestService;
    }
    
    [HttpGet("test-album")]
    public async Task<ActionResult<PublicAlbumDto>> GetTestAlbum(CancellationToken ct)
    {
        var album = await _albumService.GetTestAlbumAsync(ct);
        return album is null ? NotFound(new { error = "No test album configured." }) : Ok(album);
    }
    
    [HttpPost("invite-requests")]
    public async Task<ActionResult<InviteRequestDto>> CreateInviteRequest(
        [FromBody] CreateInviteRequestBody body,
        CancellationToken ct)
    {
        try
        {
            var created = await _inviteRequestService.CreateAsync(body, ct);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}