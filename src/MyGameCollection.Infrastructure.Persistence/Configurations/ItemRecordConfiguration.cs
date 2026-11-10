using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyGameCollection.Infrastructure.Persistence.Models;

namespace MyGameCollection.Infrastructure.Persistence.Configurations;

internal sealed class ItemRecordConfiguration : IEntityTypeConfiguration<ItemRecord>
{
    public void Configure(EntityTypeBuilder<ItemRecord> builder)
    {
        builder.ToTable("items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Type).HasColumnName("item_type").HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Title).HasMaxLength(300).IsRequired();
        builder.Property(i => i.Region).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Condition).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Barcode).HasMaxLength(64);
        builder.Property(i => i.PurchasePrice).HasPrecision(10, 2);
        builder.Property(i => i.EstimatedValue).HasPrecision(10, 2);
        builder.Property(i => i.StorageLocation).HasMaxLength(200);
        builder.Property(i => i.Notes).HasMaxLength(4000);
        builder.Property(i => i.CoverImageUrl).HasMaxLength(2000);
        builder.Property(i => i.Publisher).HasMaxLength(200);
        builder.Property(i => i.Developer).HasMaxLength(200);
        builder.Property(i => i.Genre).HasMaxLength(200);
        builder.Property(i => i.Model).HasMaxLength(100);
        builder.Property(i => i.SerialNumber).HasMaxLength(100);
        builder.Property(i => i.Edition).HasMaxLength(100);

        builder.HasOne(i => i.Platform)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Photos)
            .WithOne()
            .HasForeignKey(p => p.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.Barcode);
        builder.HasIndex(i => i.Title);
        builder.HasIndex(i => i.PlatformId);
    }
}
