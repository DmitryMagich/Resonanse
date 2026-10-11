using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resonanse.Domain.Entities;

namespace Resonanse.Infrastructure.Persistence.Configurations;

public class PeerConfiguration : IEntityTypeConfiguration<Peer>
{
    public void Configure(EntityTypeBuilder<Peer> builder)
    {
        builder.ToTable("Peers");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.PublicKey).HasMaxLength(200);
        builder.Property(p => p.PeerId).HasMaxLength(200);
        builder.Property(p => p.IsLocal).HasDefaultValue(false).IsRequired();
        builder.Property(p => p.IsBootstrap).HasDefaultValue(false).IsRequired();
        builder.Property(p => p.IsOnline).IsRequired();
        builder.Property(p => p.LastSeenAt).IsRequired();

        // Partial unique index on PublicKey when not null.
        builder.HasIndex(p => p.PublicKey)
            .IsUnique()
            .HasFilter("\"PublicKey\" IS NOT NULL");

        // Partial unique index on PeerId when not null.
        builder.HasIndex(p => p.PeerId)
            .IsUnique()
            .HasFilter("\"PeerId\" IS NOT NULL");

        // Only one local peer per device.
        builder.HasIndex(p => p.IsLocal)
            .IsUnique()
            .HasFilter("\"IsLocal\" = TRUE");

        builder.HasIndex(p => new { p.IsOnline, p.LastSeenAt });
    }
}