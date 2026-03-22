using HycoatApi.Models.MaterialInward;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.MaterialInward;

public class MaterialInwardConfiguration : IEntityTypeConfiguration<Models.MaterialInward.MaterialInward>
{
    public void Configure(EntityTypeBuilder<Models.MaterialInward.MaterialInward> builder)
    {
        builder.Property(e => e.InwardNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.InwardNumber).IsUnique();

        builder.Property(e => e.CustomerDCNumber).HasMaxLength(100);
        builder.Property(e => e.VehicleNumber).HasMaxLength(20);
        builder.Property(e => e.UnloadingLocation).HasMaxLength(100);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.MaterialInwards)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WorkOrder)
            .WithMany(wo => wo.MaterialInwards)
            .HasForeignKey(e => e.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProcessType)
            .WithMany()
            .HasForeignKey(e => e.ProcessTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PowderColor)
            .WithMany()
            .HasForeignKey(e => e.PowderColorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReceivedByUser)
            .WithMany()
            .HasForeignKey(e => e.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
