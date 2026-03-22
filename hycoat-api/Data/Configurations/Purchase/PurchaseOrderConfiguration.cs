using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Purchase;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.Property(e => e.PONumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.PONumber).IsUnique();

        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasOne(e => e.Vendor)
            .WithMany(v => v.PurchaseOrders)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PowderIndent)
            .WithMany()
            .HasForeignKey(e => e.PowderIndentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
