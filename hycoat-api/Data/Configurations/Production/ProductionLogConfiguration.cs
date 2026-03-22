using HycoatApi.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Production;

public class ProductionLogConfiguration : IEntityTypeConfiguration<ProductionLog>
{
    public void Configure(EntityTypeBuilder<ProductionLog> builder)
    {
        builder.Property(e => e.Shift).IsRequired().HasMaxLength(10);
        builder.Property(e => e.ConveyorSpeedMtrPerMin).HasPrecision(8, 2);
        builder.Property(e => e.OvenTemperature).HasPrecision(6, 2);
        builder.Property(e => e.PowderBatchNo).HasMaxLength(50);
        builder.Property(e => e.Remarks).HasMaxLength(1000);

        builder.HasOne(e => e.ProductionWorkOrder)
            .WithMany(pwo => pwo.ProductionLogs)
            .HasForeignKey(e => e.ProductionWorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SupervisorUser)
            .WithMany()
            .HasForeignKey(e => e.SupervisorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
