using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resonanse.Domain.Entities;

namespace Resonanse.Infrastructure.Persistence.Configurations;

public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("Tracks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).IsRequired().HasMaxLength(300);

        builder.Property(t => t.Duration)
            .HasConversion(
                v => (long)v.TotalMilliseconds,
                v => TimeSpan.FromMilliseconds(v))
            .IsRequired();

        builder.Property(t => t.TrackNumber).IsRequired();
        builder.Property(t => t.DiscNumber).HasDefaultValue(1).IsRequired();

        builder.HasIndex(t => t.Title);
        builder.HasIndex(t => t.AlbumId);
        builder.HasIndex(t => t.ArtistId);

        builder.HasMany(t => t.Files)
            .WithOne(f => f.Track)
            .HasForeignKey(f => f.TrackId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}