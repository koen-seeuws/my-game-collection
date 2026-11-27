using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyGameCollection.Infrastructure.EFC.DataModel;

namespace MyGameCollection.Infrastructure.EFC.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.ConfigureCollectionItem();

        builder.Property(g => g.Title).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Platform).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Edition).HasMaxLength(100);
    }
}
