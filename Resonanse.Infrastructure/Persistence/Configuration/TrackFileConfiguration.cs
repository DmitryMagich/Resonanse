using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resonanse.Domain.Entities;

namespace Resonanse.Infrastructure.Persistence.Configurations;

public class TrackFileConfiguration : IEntityTypeConfiguration<TrackFile>
{
    public void Configure(EntityTypeBuilder<TrackFile> builder)
    {
        builder.ToTable("TrackFiles");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FilePath).IsRequired().HasMaxLength(1000);
        builder.Property(f => f.FileHash).IsRequired().HasMaxLength(64);
        builder.Property(f => f.FileSize).IsRequired();
        builder.Property(f => f.Format).HasConversion<int>().IsRequired();
        builder.Property(f => f.Bitrate).IsRequired();
        builder.Property(f => f.SampleRate).IsRequired();
        builder.Property(f => f.BitDepth);
        builder.Property(f => f.Channels).IsRequired();

        builder.HasIndex(f => f.FileHash);
        builder.HasIndex(f => f.TrackId);
        builder.HasIndex(f => f.PeerId);
        builder.HasIndex(f => new { f.PeerId, f.FilePath }).IsUnique();
        builder.HasIndex(f => new { f.TrackId, f.Format });

        builder.HasOne(f => f.Peer)
            .WithMany(p => p.SharedFiles)
            .HasForeignKey(f => f.PeerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}