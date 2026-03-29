using HycoatApi.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Production;

public class YieldReportConfiguration : IEntityTypeConfiguration<YieldReport>
{
    public void Configure(EntityTypeBuilder<YieldReport> builder)
    {
        builder.Property(e => e.Shift).IsRequired().HasMaxLength(10);

        builder.Property(e => e.ProductionSFT).HasPrecision(18, 2);
        builder.Property(e => e.RejectionSFT).HasPrecision(18, 2);

        builder.Property(e => e.ElectricityOpeningKwh).HasPrecision(18, 3);
        builder.Property(e => e.ElectricityClosingKwh).HasPrecision(18, 3);
        builder.Property(e => e.ElectricityRatePerKwh).HasPrecision(18, 4);

        builder.Property(e => e.OvenGasOpeningReading).HasPrecision(18, 3);
        builder.Property(e => e.OvenGasClosingReading).HasPrecision(18, 3);
        builder.Property(e => e.OvenGasRatePerUnit).HasPrecision(18, 4);

        builder.Property(e => e.PowderUsedKg).HasPrecision(18, 3);
        builder.Property(e => e.PowderRatePerKg).HasPrecision(18, 4);

        builder.Property(e => e.ManpowerCost).HasPrecision(18, 2);
        builder.Property(e => e.OtherCost).HasPrecision(18, 2);
        builder.Property(e => e.SellingPricePerSFT).HasPrecision(18, 4);

        builder.Property(e => e.Remarks).HasMaxLength(1000);

        builder.HasOne(e => e.ProductionUnit)
            .WithMany()
            .HasForeignKey(e => e.ProductionUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.Date, e.Shift, e.ProductionUnitId })
            .IsUnique();
    }
}