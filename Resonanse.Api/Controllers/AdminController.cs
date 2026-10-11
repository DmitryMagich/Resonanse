using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos;
using Resonanse.Application.Dtos.Auth;
using Resonanse.Application.Services;

namespace Resonanse.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IInviteService _invites;
    private readonly IInviteRequestService _inviteRequests;
    private readonly IResonanseDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AdminController(
        IInviteService invites,
        IInviteRequestService inviteRequests,
        IResonanseDbContext db,
        ICurrentUser currentUser)
    {
        _invites = invites;
        _inviteRequests = inviteRequests;
        _db = db;
        _currentUser = currentUser;
    }

    // ---------- INVITES ----------

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
    public async Task<IActionResult> RevokeInvite(Guid id, CancellationToken ct)
    {
        var ok = await _invites.RevokeAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    // ---------- INVITE REQUESTS ----------

    [HttpGet("invite-requests")]
    public async Task<ActionResult<List<InviteRequestDto>>> ListInviteRequests(
        [FromQuery] string? status,
        CancellationToken ct)
    {
        return Ok(await _inviteRequests.ListAsync(status, ct));
    }

    [HttpPost("invite-requests/{id:guid}/approve")]
    public async Task<ActionResult<InviteRequestDto>> ApproveInviteRequest(Guid id, CancellationToken ct)
    {
        var adminId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user not found.");
        try
        {
            var result = await _inviteRequests.ApproveAsync(id, adminId, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("invite-requests/{id:guid}/reject")]
    public async Task<ActionResult<InviteRequestDto>> RejectInviteRequest(Guid id, CancellationToken ct)
    {
        var adminId = _currentUser.UserId
            ?? throw new InvalidOperationException("Current user not found.");
        try
        {
            var result = await _inviteRequests.RejectAsync(id, adminId, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ---------- TEST ALBUM ----------
    
    [HttpPost("albums/{id:guid}/mark-test")]
    public async Task<IActionResult> MarkTestAlbum(Guid id, CancellationToken ct)
    {
        var all = await _db.Albums.ToListAsync(ct);
        foreach (var a in all)
            a.IsTestAlbum = false;

        var album = all.FirstOrDefault(a => a.Id == id);
        if (album is null) return NotFound();

        album.IsTestAlbum = true;
        album.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { albumId = album.Id, title = album.Title, isTestAlbum = true });
    }

    /// <summary>
    /// Снять флаг «тестовый альбом».
    /// </summary>
    [HttpPost("albums/{id:guid}/unmark-test")]
    public async Task<IActionResult> UnmarkTestAlbum(Guid id, CancellationToken ct)
    {
        var album = await _db.Albums.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (album is null) return NotFound();

        album.IsTestAlbum = false;
        album.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { albumId = album.Id, isTestAlbum = false });
    }
    
    [HttpGet("test-album")]
    public async Task<IActionResult> GetCurrentTestAlbum(CancellationToken ct)
    {
        var album = await _db.Albums
            .AsNoTracking()
            .Where(a => a.IsTestAlbum)
            .Select(a => new { a.Id, a.Title, a.ReleaseYear })
            .FirstOrDefaultAsync(ct);

        return album is null
            ? Ok(new { albumId = (Guid?)null, message = "No test album configured." })
            : Ok(new { albumId = (Guid?)album.Id, album.Title, album.ReleaseYear });
    }
}