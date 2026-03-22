using HycoatApi.Models.Dispatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Dispatch;

public class PackingListConfiguration : IEntityTypeConfiguration<PackingList>
{
    public void Configure(EntityTypeBuilder<PackingList> builder)
    {
        builder.Property(e => e.PackingType).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasOne(e => e.ProductionWorkOrder)
            .WithMany()
            .HasForeignKey(e => e.ProductionWorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WorkOrder)
            .WithMany()
            .HasForeignKey(e => e.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PreparedByUser)
            .WithMany()
            .HasForeignKey(e => e.PreparedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
