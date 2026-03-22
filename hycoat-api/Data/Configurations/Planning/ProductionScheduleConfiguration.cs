using HycoatApi.Models.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Planning;

public class ProductionScheduleConfiguration : IEntityTypeConfiguration<ProductionSchedule>
{
    public void Configure(EntityTypeBuilder<ProductionSchedule> builder)
    {
        builder.Property(e => e.Shift).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.ProductionWorkOrder)
            .WithMany(pwo => pwo.Schedules)
            .HasForeignKey(e => e.ProductionWorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProductionUnit)
            .WithMany()
            .HasForeignKey(e => e.ProductionUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.Date, e.Shift, e.ProductionUnitId });
    }
}
