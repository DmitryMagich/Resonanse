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

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PublicKey)
            .HasMaxLength(200); // в мвп без этого

        builder.Property(p => p.IsOnline)
            .IsRequired();

        builder.Property(p => p.LastSeenAt)
            .IsRequired();
        
        builder.HasIndex(p => p.PublicKey)
            .IsUnique()
            .HasFilter("\"PublicKey\" IS NOT NULL");
        
        builder.HasIndex(p => new { p.IsOnline, p.LastSeenAt });
    }
}