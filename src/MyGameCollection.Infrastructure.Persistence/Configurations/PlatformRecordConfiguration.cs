using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyGameCollection.Infrastructure.Persistence.Models;

namespace MyGameCollection.Infrastructure.Persistence.Configurations;

internal sealed class PlatformRecordConfiguration : IEntityTypeConfiguration<PlatformRecord>
{
    public void Configure(EntityTypeBuilder<PlatformRecord> builder)
    {
        builder.ToTable("platforms");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Manufacturer).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.Name).IsUnique();
    }
}
