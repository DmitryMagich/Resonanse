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

        builder.Property(f => f.Source).HasConversion<int>().IsRequired().HasDefaultValue(Resonanse.Domain.Enums.TrackFileSource.Local);
        builder.Property(f => f.ExternalId).HasMaxLength(500);
        builder.Property(f => f.ExpiresAt);
        builder.Property(f => f.IsPinnedByUser).HasDefaultValue(false);
        builder.Property(f => f.ExternalMetadataJson).HasColumnType("text");

        builder.HasIndex(f => f.FileHash);
        builder.HasIndex(f => f.TrackId);
        builder.HasIndex(f => f.PeerId);
        builder.HasIndex(f => f.Source);
        builder.HasIndex(f => f.ExpiresAt);
        builder.HasIndex(f => new { f.PeerId, f.FilePath }).IsUnique();
        builder.HasIndex(f => new { f.TrackId, f.Format });

        builder.HasOne(f => f.Peer)
            .WithMany(p => p.SharedFiles)
            .HasForeignKey(f => f.PeerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}