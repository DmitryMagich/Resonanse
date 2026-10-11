using Microsoft.EntityFrameworkCore;
using Resonanse.Domain.Entities;
using Resonanse.Infrastructure.Persistence;

namespace Resonanse.Api.Startup;

public static class PeerSeeder
{
    /// <summary>
    /// Ensures a local peer exists. Promotes it to local if needed.
    /// Only one peer can be local at a time.
    /// </summary>
    public static async Task EnsureLocalPeerAsync(IServiceProvider services, string peerName, bool isBootstrap)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResonanseDbContext>();

        // Demote any existing local peers
        var locals = await db.Peers.Where(p => p.IsLocal).ToListAsync();
        foreach (var p in locals)
            p.IsLocal = false;

        var existing = await db.Peers.FirstOrDefaultAsync(p => p.Name == peerName);
        if (existing is not null)
        {
            existing.IsLocal = true;
            existing.IsBootstrap = isBootstrap;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            db.Peers.Add(new Peer
            {
                Name = peerName,
                IsLocal = true,
                IsBootstrap = isBootstrap,
                IsOnline = true,
                LastSeenAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }
}