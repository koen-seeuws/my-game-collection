using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyGameCollection.Infrastructure.EFC.DataModel;

namespace MyGameCollection.Infrastructure.EFC.Configurations;

public class GameCollectionConfiguration : IEntityTypeConfiguration<GameCollection>
{
    public void Configure(EntityTypeBuilder<GameCollection> builder)
    {
        builder.ToTable("GameCollections");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Owner).HasMaxLength(256).IsRequired();
        builder.HasIndex(c => c.Owner).IsUnique();

        builder.HasMany(c => c.Games)
            .WithOne(g => g.GameCollection)
            .HasForeignKey(g => g.GameCollectionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Consoles)
            .WithOne(g => g.GameCollection)
            .HasForeignKey(g => g.GameCollectionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
