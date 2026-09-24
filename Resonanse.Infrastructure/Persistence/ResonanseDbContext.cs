using Microsoft.EntityFrameworkCore;
using Resonanse.Domain.Entities;

namespace Resonanse.Infrastructure.Persistence;

public class ResonanseDbContext : DbContext
{
    public ResonanseDbContext(DbContextOptions<ResonanseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Peer> Peers => Set<Peer>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<TrackFile> TrackFiles => Set<TrackFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ResonanseDbContext).Assembly);
    }
}