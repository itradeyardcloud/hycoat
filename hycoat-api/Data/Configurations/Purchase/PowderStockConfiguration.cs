using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Purchase;

public class PowderStockConfiguration : IEntityTypeConfiguration<PowderStock>
{
    public void Configure(EntityTypeBuilder<PowderStock> builder)
    {
        builder.HasIndex(e => e.PowderColorId).IsUnique();

        builder.Property(e => e.CurrentStockKg).HasPrecision(12, 2);
        builder.Property(e => e.ReorderLevelKg).HasPrecision(10, 2);

        builder.HasOne(e => e.PowderColor)
            .WithMany()
            .HasForeignKey(e => e.PowderColorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
