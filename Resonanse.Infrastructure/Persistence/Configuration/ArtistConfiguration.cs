using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resonanse.Domain.Entities;

namespace Resonanse.Infrastructure.Persistence.Configurations;

public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("Artists");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Bio)
            .HasColumnType("text");

        builder.Property(a => a.ImagePath)
            .HasMaxLength(1000);

        builder.HasIndex(a => a.Name);

        builder.HasMany(a => a.Albums)
            .WithOne(al => al.Artist)
            .HasForeignKey(al => al.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Tracks)
            .WithOne(t => t.Artist)
            .HasForeignKey(t => t.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}