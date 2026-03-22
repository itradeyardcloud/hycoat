using HycoatApi.Models.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Planning;

public class ProductionWorkOrderConfiguration : IEntityTypeConfiguration<ProductionWorkOrder>
{
    public void Configure(EntityTypeBuilder<ProductionWorkOrder> builder)
    {
        builder.Property(e => e.PWONumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.PWONumber).IsUnique();

        builder.Property(e => e.PowderCode).HasMaxLength(50);
        builder.Property(e => e.ColorName).HasMaxLength(100);
        builder.Property(e => e.PreTreatmentTimeHrs).HasPrecision(8, 2);
        builder.Property(e => e.PostTreatmentTimeHrs).HasPrecision(8, 2);
        builder.Property(e => e.TotalTimeHrs).HasPrecision(8, 2);
        builder.Property(e => e.ShiftAllocation).HasMaxLength(20);
        builder.Property(e => e.PackingType).HasMaxLength(100);
        builder.Property(e => e.SpecialInstructions).HasMaxLength(2000);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);

        builder.HasOne(e => e.WorkOrder)
            .WithMany(wo => wo.ProductionWorkOrders)
            .HasForeignKey(e => e.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProcessType)
            .WithMany()
            .HasForeignKey(e => e.ProcessTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PowderColor)
            .WithMany()
            .HasForeignKey(e => e.PowderColorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProductionUnit)
            .WithMany()
            .HasForeignKey(e => e.ProductionUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
