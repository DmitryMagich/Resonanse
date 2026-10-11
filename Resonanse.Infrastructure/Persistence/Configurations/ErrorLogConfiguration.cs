using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resonanse.Domain.Entities;

namespace Resonanse.Infrastructure.Persistence.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("ErrorLogs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Level).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.StackTrace).HasColumnType("text");
        builder.Property(e => e.ExceptionType).HasMaxLength(500);
        builder.Property(e => e.Path).HasMaxLength(1000);
        builder.Property(e => e.Method).HasMaxLength(20);
        builder.Property(e => e.Username).HasMaxLength(200);
        builder.Property(e => e.IpAddress).HasMaxLength(100);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.QueryString).HasMaxLength(2000);
        builder.Property(e => e.Category).HasMaxLength(500);
        builder.Property(e => e.AdditionalData).HasColumnType("text");

        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.Level);
        builder.HasIndex(e => e.UserId);
    }
}