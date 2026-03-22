using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Sales;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.Property(e => e.WONumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.WONumber).IsUnique();

        builder.Property(e => e.ProjectName).HasMaxLength(300);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.WorkOrders)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProformaInvoice)
            .WithMany()
            .HasForeignKey(e => e.ProformaInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProcessType)
            .WithMany()
            .HasForeignKey(e => e.ProcessTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PowderColor)
            .WithMany()
            .HasForeignKey(e => e.PowderColorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
