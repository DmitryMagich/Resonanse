using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Application.Dtos.Auth;
using Resonanse.Domain.Entities;

namespace Resonanse.Application.Services;

public class InviteService : IInviteService
{
    private readonly IResonanseDbContext _db;

    public InviteService(IResonanseDbContext db)
    {
        _db = db;
    }

    public async Task<InviteDto> CreateAsync(Guid adminUserId, int validDays, CancellationToken ct = default)
    {
        validDays = Math.Clamp(validDays, 1, 365);

        var invite = new Invite
        {
            Code = GenerateCode(),
            CreatedByUserId = adminUserId,
            ExpiresAt = DateTime.UtcNow.AddDays(validDays)
        };
        _db.Invites.Add(invite);
        await _db.SaveChangesAsync(ct);

        return await MapAsync(invite.Id, ct);
    }

    public async Task<List<InviteDto>> ListAsync(CancellationToken ct = default)
    {
        return await _db.Invites
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InviteDto
            {
                Id = i.Id,
                Code = i.Code,
                CreatedByUserId = i.CreatedByUserId,
                CreatedByUsername = i.CreatedBy.Username,
                UsedByUserId = i.UsedByUserId,
                UsedByUsername = i.UsedBy != null ? i.UsedBy.Username : null,
                UsedAt = i.UsedAt,
                ExpiresAt = i.ExpiresAt,
                IsActive = i.UsedAt == null && i.ExpiresAt > DateTime.UtcNow
            })
            .ToListAsync(ct);
    }

    public async Task<bool> RevokeAsync(Guid inviteId, CancellationToken ct = default)
    {
        var invite = await _db.Invites.FirstOrDefaultAsync(i => i.Id == inviteId, ct);
        if (invite is null) return false;

        invite.ExpiresAt = DateTime.UtcNow;
        invite.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<InviteDto> MapAsync(Guid id, CancellationToken ct)
    {
        return await _db.Invites
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InviteDto
            {
                Id = i.Id,
                Code = i.Code,
                CreatedByUserId = i.CreatedByUserId,
                CreatedByUsername = i.CreatedBy.Username,
                UsedByUserId = i.UsedByUserId,
                UsedByUsername = i.UsedBy != null ? i.UsedBy.Username : null,
                UsedAt = i.UsedAt,
                ExpiresAt = i.ExpiresAt,
                IsActive = i.UsedAt == null && i.ExpiresAt > DateTime.UtcNow
            })
            .FirstAsync(ct);
    }

    private static string GenerateCode()
    {
        var bytes = Guid.NewGuid().ToByteArray();
        var hex = Convert.ToHexString(bytes).ToLowerInvariant();
        return $"{hex[..4]}-{hex[4..8]}-{hex[8..12]}-{hex[12..16]}";
    }
} // thx AI :,) for this file :P