using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyGameCollection.Infrastructure.Persistence.Models;

namespace MyGameCollection.Infrastructure.Persistence.Configurations;

internal sealed class ItemPhotoRecordConfiguration : IEntityTypeConfiguration<ItemPhotoRecord>
{
    public void Configure(EntityTypeBuilder<ItemPhotoRecord> builder)
    {
        builder.ToTable("item_photos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.FileName).HasMaxLength(300).IsRequired();
        builder.Property(p => p.ThumbnailFileName).HasMaxLength(300).IsRequired();
        builder.Property(p => p.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(p => p.OriginalFileName).HasMaxLength(300);
        builder.HasIndex(p => p.ItemId);
    }
}
