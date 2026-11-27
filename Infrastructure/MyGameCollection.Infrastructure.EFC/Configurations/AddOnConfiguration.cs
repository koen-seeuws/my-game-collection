using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyGameCollection.Infrastructure.EFC.DataModel;

namespace MyGameCollection.Infrastructure.EFC.Configurations;

public class AddOnConfiguration : IEntityTypeConfiguration<AddOn>
{
    public void Configure(EntityTypeBuilder<AddOn> builder)
    {
        builder.ToTable("AddOns");

        builder.ConfigureItemRow();

        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Format).HasConversion<string>().HasMaxLength(20);
    }
}
