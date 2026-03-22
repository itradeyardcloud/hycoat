using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Sales;

public class QuotationLineItemConfiguration : IEntityTypeConfiguration<QuotationLineItem>
{
    public void Configure(EntityTypeBuilder<QuotationLineItem> builder)
    {
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.RatePerSFT).HasPrecision(10, 2);
        builder.Property(e => e.MicronRange).HasMaxLength(30);

        builder.HasOne(e => e.Quotation)
            .WithMany(q => q.LineItems)
            .HasForeignKey(e => e.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ProcessType)
            .WithMany()
            .HasForeignKey(e => e.ProcessTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
