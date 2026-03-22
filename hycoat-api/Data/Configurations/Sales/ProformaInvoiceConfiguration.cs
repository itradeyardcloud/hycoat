using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Sales;

public class ProformaInvoiceConfiguration : IEntityTypeConfiguration<ProformaInvoice>
{
    public void Configure(EntityTypeBuilder<ProformaInvoice> builder)
    {
        builder.Property(e => e.PINumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.PINumber).IsUnique();

        builder.Property(e => e.CustomerAddress).HasMaxLength(500);
        builder.Property(e => e.CustomerGSTIN).HasMaxLength(15);
        builder.Property(e => e.SubTotal).HasPrecision(14, 2);
        builder.Property(e => e.PackingCharges).HasPrecision(10, 2);
        builder.Property(e => e.TransportCharges).HasPrecision(10, 2);
        builder.Property(e => e.TaxableAmount).HasPrecision(14, 2);
        builder.Property(e => e.CGSTRate).HasPrecision(5, 2);
        builder.Property(e => e.CGSTAmount).HasPrecision(12, 2);
        builder.Property(e => e.SGSTRate).HasPrecision(5, 2);
        builder.Property(e => e.SGSTAmount).HasPrecision(12, 2);
        builder.Property(e => e.IGSTRate).HasPrecision(5, 2);
        builder.Property(e => e.IGSTAmount).HasPrecision(12, 2);
        builder.Property(e => e.GrandTotal).HasPrecision(14, 2);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.FileUrl).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Quotation)
            .WithMany()
            .HasForeignKey(e => e.QuotationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PreparedByUser)
            .WithMany()
            .HasForeignKey(e => e.PreparedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
