using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Quality;

public class InProcessInspectionConfiguration : IEntityTypeConfiguration<InProcessInspection>
{
    public void Configure(EntityTypeBuilder<InProcessInspection> builder)
    {
        builder.Property(e => e.Remarks).HasMaxLength(1000);

        builder.HasOne(e => e.ProductionWorkOrder)
            .WithMany(pwo => pwo.InProcessInspections)
            .HasForeignKey(e => e.ProductionWorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InspectorUser)
            .WithMany()
            .HasForeignKey(e => e.InspectorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
