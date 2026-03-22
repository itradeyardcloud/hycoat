using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Purchase;

public class POLineItemConfiguration : IEntityTypeConfiguration<POLineItem>
{
    public void Configure(EntityTypeBuilder<POLineItem> builder)
    {
        builder.Property(e => e.QtyKg).HasPrecision(10, 2);
        builder.Property(e => e.RatePerKg).HasPrecision(10, 2);
        builder.Property(e => e.Amount).HasPrecision(12, 2);

        builder.HasOne(e => e.PurchaseOrder)
            .WithMany(po => po.LineItems)
            .HasForeignKey(e => e.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PowderColor)
            .WithMany()
            .HasForeignKey(e => e.PowderColorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
