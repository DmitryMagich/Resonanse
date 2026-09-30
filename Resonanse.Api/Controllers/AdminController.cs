using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos.Auth;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IInviteService _invites;
    private readonly ICurrentUser _currentUser;

    public AdminController(IInviteService invites, ICurrentUser currentUser)
    {
        _invites = invites;
        _currentUser = currentUser;
    }

    [HttpPost("invites")]
    public async Task<ActionResult<InviteDto>> CreateInvite(
        [FromBody] CreateInviteRequest request,
        CancellationToken ct)
    {
        var adminId = _currentUser.UserId
                      ?? throw new InvalidOperationException("Current user not found.");
        return Ok(await _invites.CreateAsync(adminId, request.ValidDays, ct));
    }

    [HttpGet("invites")]
    public async Task<ActionResult<List<InviteDto>>> ListInvites(CancellationToken ct)
    {
        return Ok(await _invites.ListAsync(ct));
    }

    [HttpDelete("invites/{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var ok = await _invites.RevokeAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}