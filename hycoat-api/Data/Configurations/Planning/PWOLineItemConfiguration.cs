using HycoatApi.Models.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Planning;

public class PWOLineItemConfiguration : IEntityTypeConfiguration<PWOLineItem>
{
    public void Configure(EntityTypeBuilder<PWOLineItem> builder)
    {
        builder.Property(e => e.CustomerDCNo).HasMaxLength(50);
        builder.Property(e => e.LengthMM).HasPrecision(10, 2);
        builder.Property(e => e.PerimeterMM).HasPrecision(10, 2);
        builder.Property(e => e.UnitSurfaceAreaSqMtr).HasPrecision(10, 4);
        builder.Property(e => e.TotalSurfaceAreaSqft).HasPrecision(12, 2);
        builder.Property(e => e.SpecialInstructions).HasMaxLength(500);

        builder.HasOne(e => e.ProductionWorkOrder)
            .WithMany(pwo => pwo.LineItems)
            .HasForeignKey(e => e.ProductionWorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SectionProfile)
            .WithMany()
            .HasForeignKey(e => e.SectionProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
