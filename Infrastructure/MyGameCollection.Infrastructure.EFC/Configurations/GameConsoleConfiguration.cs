using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyGameCollection.Infrastructure.EFC.DataModel;

namespace MyGameCollection.Infrastructure.EFC.Configurations;

public class GameConsoleConfiguration : IEntityTypeConfiguration<GameConsole>
{
    public void Configure(EntityTypeBuilder<GameConsole> builder)
    {
        builder.ToTable("Consoles");

        builder.ConfigureCollectionItem();

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Manufacturer).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Model).HasMaxLength(100);
    }
}
