using Microsoft.EntityFrameworkCore;
using Resonanse.Domain.Entities;

namespace Resonanse.Application.Abstractions;

public interface IResonanseDbContext
{
    DbSet<Peer> Peers { get; }
    DbSet<Artist> Artists { get; }
    DbSet<Album> Albums { get; }
    DbSet<Track> Tracks { get; }
    DbSet<TrackFile> TrackFiles { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Invite> Invites { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}