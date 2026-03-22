using HycoatApi.Models.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Planning;

public class ProductionTimeCalcConfiguration : IEntityTypeConfiguration<ProductionTimeCalc>
{
    public void Configure(EntityTypeBuilder<ProductionTimeCalc> builder)
    {
        builder.Property(e => e.ThicknessMM).HasPrecision(6, 2);
        builder.Property(e => e.WidthMM).HasPrecision(10, 2);
        builder.Property(e => e.HeightMM).HasPrecision(10, 2);
        builder.Property(e => e.SpecificWeight).HasPrecision(8, 4);
        builder.Property(e => e.WeightPerMtr).HasPrecision(10, 4);
        builder.Property(e => e.TotalWeightKg).HasPrecision(12, 2);
        builder.Property(e => e.TotalTimePreTreatMins).HasPrecision(10, 2);
        builder.Property(e => e.ConveyorSpeedMtrPerMin).HasPrecision(8, 2);
        builder.Property(e => e.JigLengthMM).HasPrecision(10, 2);
        builder.Property(e => e.GapBetweenJigsMM).HasPrecision(10, 2);
        builder.Property(e => e.TotalConveyorDistanceMtrs).HasPrecision(12, 2);
        builder.Property(e => e.TotalTimePostTreatMins).HasPrecision(10, 2);

        builder.HasOne(e => e.PWOLineItem)
            .WithMany()
            .HasForeignKey(e => e.PWOLineItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
