using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resonanse.Domain.Entities;

namespace Resonanse.Infrastructure.Persistence.Configurations;

public class ScanJobConfiguration : IEntityTypeConfiguration<ScanJob>
{
    public void Configure(EntityTypeBuilder<ScanJob> builder)
    {
        builder.ToTable("ScanJobs");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Path).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.Status).HasConversion<int>().IsRequired();
        builder.Property(s => s.Trigger).HasMaxLength(50);
        builder.Property(s => s.ErrorMessage).HasMaxLength(2000);
        builder.Property(s => s.ErrorMessagesJson).HasColumnType("text");

        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.StartedAt);
        builder.HasIndex(s => s.StartedByUserId);

        builder.HasOne(s => s.StartedBy)
            .WithMany()
            .HasForeignKey(s => s.StartedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}