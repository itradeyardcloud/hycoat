using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Sales;

public class PILineItemConfiguration : IEntityTypeConfiguration<PILineItem>
{
    public void Configure(EntityTypeBuilder<PILineItem> builder)
    {
        builder.Property(e => e.SectionNumber).HasMaxLength(50);
        builder.Property(e => e.LengthMM).HasPrecision(10, 2);
        builder.Property(e => e.PerimeterMM).HasPrecision(10, 2);
        builder.Property(e => e.AreaSqMtr).HasPrecision(12, 4);
        builder.Property(e => e.AreaSFT).HasPrecision(12, 2);
        builder.Property(e => e.RatePerSFT).HasPrecision(10, 2);
        builder.Property(e => e.Amount).HasPrecision(14, 2);

        builder.HasOne(e => e.ProformaInvoice)
            .WithMany(pi => pi.LineItems)
            .HasForeignKey(e => e.ProformaInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SectionProfile)
            .WithMany()
            .HasForeignKey(e => e.SectionProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
