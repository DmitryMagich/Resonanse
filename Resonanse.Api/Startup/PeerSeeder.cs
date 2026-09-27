using Microsoft.EntityFrameworkCore;
using Resonanse.Domain.Entities;
using Resonanse.Infrastructure.Persistence;

namespace Resonanse.Api.Startup;

public static class PeerSeeder
{
    public static async Task EnsureLocalPeerAsync(IServiceProvider services, string peerName)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResonanseDbContext>();

        var exists = await db.Peers.AnyAsync(p => p.Name == peerName);
        if (exists) return;

        db.Peers.Add(new Peer
        {
            Name = peerName,
            IsOnline = true,
            LastSeenAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}