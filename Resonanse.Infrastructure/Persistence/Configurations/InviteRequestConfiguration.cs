using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resonanse.Domain.Entities;

namespace Resonanse.Infrastructure.Persistence.Configurations;

public class InviteRequestConfiguration : IEntityTypeConfiguration<InviteRequest>
{
    public void Configure(EntityTypeBuilder<InviteRequest> builder)
    {
        builder.ToTable("InviteRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Comment).HasColumnType("text");
        builder.Property(r => r.Contact).HasMaxLength(500);
        builder.Property(r => r.Status).HasConversion<int>().IsRequired();
        builder.Property(r => r.ProcessedAt);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedAt);

        builder.HasOne(r => r.ProcessedBy)
            .WithMany()
            .HasForeignKey(r => r.ProcessedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}