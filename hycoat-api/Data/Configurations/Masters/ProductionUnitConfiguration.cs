using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Masters;

public class ProductionUnitConfiguration : IEntityTypeConfiguration<ProductionUnit>
{
    public void Configure(EntityTypeBuilder<ProductionUnit> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.TankLengthMM).HasPrecision(10, 2);
        builder.Property(e => e.TankWidthMM).HasPrecision(10, 2);
        builder.Property(e => e.TankHeightMM).HasPrecision(10, 2);
        builder.Property(e => e.BucketLengthMM).HasPrecision(10, 2);
        builder.Property(e => e.BucketWidthMM).HasPrecision(10, 2);
        builder.Property(e => e.BucketHeightMM).HasPrecision(10, 2);
        builder.Property(e => e.ConveyorLengthMtrs).HasPrecision(10, 2);
    }
}
