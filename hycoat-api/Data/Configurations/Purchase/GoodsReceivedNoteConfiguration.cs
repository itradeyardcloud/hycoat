using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Purchase;

public class GoodsReceivedNoteConfiguration : IEntityTypeConfiguration<GoodsReceivedNote>
{
    public void Configure(EntityTypeBuilder<GoodsReceivedNote> builder)
    {
        builder.Property(e => e.GRNNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.GRNNumber).IsUnique();

        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasOne(e => e.PurchaseOrder)
            .WithMany()
            .HasForeignKey(e => e.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReceivedByUser)
            .WithMany()
            .HasForeignKey(e => e.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
