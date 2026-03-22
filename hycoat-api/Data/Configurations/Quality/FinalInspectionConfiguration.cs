using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Quality;

public class FinalInspectionConfiguration : IEntityTypeConfiguration<FinalInspection>
{
    public void Configure(EntityTypeBuilder<FinalInspection> builder)
    {
        builder.Property(e => e.InspectionNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.InspectionNumber).IsUnique();

        builder.Property(e => e.VisualCheckStatus).HasMaxLength(10);
        builder.Property(e => e.DFTRecheckStatus).HasMaxLength(10);
        builder.Property(e => e.ShadeMatchFinalStatus).HasMaxLength(10);
        builder.Property(e => e.OverallStatus).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Remarks).HasMaxLength(2000);

        builder.HasOne(e => e.ProductionWorkOrder)
            .WithMany(pwo => pwo.FinalInspections)
            .HasForeignKey(e => e.ProductionWorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InspectorUser)
            .WithMany()
            .HasForeignKey(e => e.InspectorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
