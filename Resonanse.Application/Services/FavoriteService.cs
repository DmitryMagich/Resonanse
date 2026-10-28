using Microsoft.EntityFrameworkCore;
using Resonanse.Application.Abstractions;
using Resonanse.Domain.Entities;

namespace Resonanse.Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IResonanseDbContext _db;
    private readonly ICurrentUser _currentUser;

    public FavoriteService(IResonanseDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<bool> ToggleAsync(Guid trackId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        var existing = await _db.UserFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.TrackId == trackId, ct);

        if (existing is not null)
        {
            _db.UserFavorites.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return false; // </3
        }
        else
        {
            _db.UserFavorites.Add(new UserFavorite { UserId = userId, TrackId = trackId });
            await _db.SaveChangesAsync(ct);
            return true; // <3
        }
    }

    public async Task<bool> IsFavoriteAsync(Guid trackId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return false;
        return await _db.UserFavorites.AnyAsync(f => f.UserId == userId.Value && f.TrackId == trackId, ct);
    }
}