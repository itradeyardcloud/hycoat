using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Purchase;

public class PowderIndentConfiguration : IEntityTypeConfiguration<PowderIndent>
{
    public void Configure(EntityTypeBuilder<PowderIndent> builder)
    {
        builder.Property(e => e.IndentNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.IndentNumber).IsUnique();

        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasOne(e => e.ProductionWorkOrder)
            .WithMany()
            .HasForeignKey(e => e.ProductionWorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RequestedByUser)
            .WithMany()
            .HasForeignKey(e => e.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
