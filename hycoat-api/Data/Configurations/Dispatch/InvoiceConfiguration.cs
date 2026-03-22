using HycoatApi.Models.Dispatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Dispatch;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.InvoiceNumber).IsUnique();

        builder.Property(e => e.CustomerName).HasMaxLength(300);
        builder.Property(e => e.CustomerAddress).HasMaxLength(500);
        builder.Property(e => e.CustomerGSTIN).HasMaxLength(15);
        builder.Property(e => e.OurGSTIN).IsRequired().HasMaxLength(15);
        builder.Property(e => e.HSNSACCode).HasMaxLength(20);
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
        builder.Property(e => e.RoundOff).HasPrecision(6, 2);
        builder.Property(e => e.AmountInWords).HasMaxLength(300);
        builder.Property(e => e.PaymentTerms).HasMaxLength(500);
        builder.Property(e => e.BankName).HasMaxLength(100);
        builder.Property(e => e.BankAccountNo).HasMaxLength(30);
        builder.Property(e => e.BankIFSC).HasMaxLength(15);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.Property(e => e.FileUrl).HasMaxLength(500);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WorkOrder)
            .WithMany(wo => wo.Invoices)
            .HasForeignKey(e => e.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DeliveryChallan)
            .WithMany()
            .HasForeignKey(e => e.DeliveryChallanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
