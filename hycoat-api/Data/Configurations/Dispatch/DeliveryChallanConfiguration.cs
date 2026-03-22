using HycoatApi.Models.Dispatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Dispatch;

public class DeliveryChallanConfiguration : IEntityTypeConfiguration<DeliveryChallan>
{
    public void Configure(EntityTypeBuilder<DeliveryChallan> builder)
    {
        builder.Property(e => e.DCNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.DCNumber).IsUnique();

        builder.Property(e => e.CustomerAddress).HasMaxLength(500);
        builder.Property(e => e.CustomerGSTIN).HasMaxLength(15);
        builder.Property(e => e.VehicleNumber).HasMaxLength(20);
        builder.Property(e => e.DriverName).HasMaxLength(100);
        builder.Property(e => e.LRNumber).HasMaxLength(50);
        builder.Property(e => e.MaterialValueApprox).HasPrecision(14, 2);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasOne(e => e.WorkOrder)
            .WithMany(wo => wo.DeliveryChallans)
            .HasForeignKey(e => e.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
