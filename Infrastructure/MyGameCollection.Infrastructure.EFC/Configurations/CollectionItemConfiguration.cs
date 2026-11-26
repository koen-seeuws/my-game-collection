using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyGameCollection.Infrastructure.EFC.DataModel;

namespace MyGameCollection.Infrastructure.EFC.Configurations;

/// <summary>Maps the columns shared by games and consoles (each still gets its own table).</summary>
internal static class CollectionItemConfiguration
{
    public static void ConfigureCollectionItem<TItem>(this EntityTypeBuilder<TItem> builder) where TItem : CollectionItem
    {
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => i.Key).IsUnique();

        // Enums are stored by name so the database stays readable.
        builder.Property(i => i.Region).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Condition).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Completeness).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(i => i.PurchasePrice).HasPrecision(10, 2);
        builder.Property(i => i.Notes).HasMaxLength(2000);
    }
}
