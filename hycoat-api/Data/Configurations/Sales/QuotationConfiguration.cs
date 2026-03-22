using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Sales;

public class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.Property(e => e.QuotationNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.QuotationNumber).IsUnique();

        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.FileUrl).HasMaxLength(500);

        builder.HasOne(e => e.Inquiry)
            .WithMany(i => i.Quotations)
            .HasForeignKey(e => e.InquiryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PreparedByUser)
            .WithMany()
            .HasForeignKey(e => e.PreparedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
