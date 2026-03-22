using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Quality;

public class PanelTestConfiguration : IEntityTypeConfiguration<PanelTest>
{
    public void Configure(EntityTypeBuilder<PanelTest> builder)
    {
        builder.Property(e => e.BoilingWaterResult).HasMaxLength(100);
        builder.Property(e => e.BoilingWaterStatus).HasMaxLength(10);
        builder.Property(e => e.ImpactTestResult).HasMaxLength(100);
        builder.Property(e => e.ImpactTestStatus).HasMaxLength(10);
        builder.Property(e => e.ConicalMandrelResult).HasMaxLength(100);
        builder.Property(e => e.ConicalMandrelStatus).HasMaxLength(10);
        builder.Property(e => e.PencilHardnessResult).HasMaxLength(100);
        builder.Property(e => e.PencilHardnessStatus).HasMaxLength(10);
        builder.Property(e => e.Remarks).HasMaxLength(1000);

        builder.HasOne(e => e.ProductionWorkOrder)
            .WithMany(pwo => pwo.PanelTests)
            .HasForeignKey(e => e.ProductionWorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InspectorUser)
            .WithMany()
            .HasForeignKey(e => e.InspectorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
