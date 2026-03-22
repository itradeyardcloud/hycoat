using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Purchase;

public class GRNLineItemConfiguration : IEntityTypeConfiguration<GRNLineItem>
{
    public void Configure(EntityTypeBuilder<GRNLineItem> builder)
    {
        builder.Property(e => e.QtyReceivedKg).HasPrecision(10, 2);
        builder.Property(e => e.BatchCode).HasMaxLength(50);

        builder.HasOne(e => e.GoodsReceivedNote)
            .WithMany(grn => grn.LineItems)
            .HasForeignKey(e => e.GoodsReceivedNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PowderColor)
            .WithMany()
            .HasForeignKey(e => e.PowderColorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
