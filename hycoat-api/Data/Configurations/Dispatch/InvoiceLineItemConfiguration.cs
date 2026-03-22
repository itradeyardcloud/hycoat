using HycoatApi.Models.Dispatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Dispatch;

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.Property(e => e.SectionNumber).HasMaxLength(50);
        builder.Property(e => e.DCNumber).HasMaxLength(30);
        builder.Property(e => e.Color).HasMaxLength(50);
        builder.Property(e => e.MicronRange).HasMaxLength(20);
        builder.Property(e => e.PerimeterMM).HasPrecision(10, 2);
        builder.Property(e => e.LengthMM).HasPrecision(10, 2);
        builder.Property(e => e.AreaSFT).HasPrecision(12, 2);
        builder.Property(e => e.RatePerSFT).HasPrecision(10, 2);
        builder.Property(e => e.Amount).HasPrecision(14, 2);

        builder.HasOne(e => e.Invoice)
            .WithMany(inv => inv.LineItems)
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SectionProfile)
            .WithMany()
            .HasForeignKey(e => e.SectionProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
