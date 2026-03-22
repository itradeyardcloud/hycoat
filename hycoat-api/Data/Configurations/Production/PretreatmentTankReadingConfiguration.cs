using HycoatApi.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Production;

public class PretreatmentTankReadingConfiguration : IEntityTypeConfiguration<PretreatmentTankReading>
{
    public void Configure(EntityTypeBuilder<PretreatmentTankReading> builder)
    {
        builder.Property(e => e.TankName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Concentration).HasPrecision(8, 4);
        builder.Property(e => e.Temperature).HasPrecision(6, 2);
        builder.Property(e => e.ChemicalAdded).HasMaxLength(200);
        builder.Property(e => e.ChemicalQty).HasPrecision(8, 2);

        builder.HasOne(e => e.PretreatmentLog)
            .WithMany(pl => pl.TankReadings)
            .HasForeignKey(e => e.PretreatmentLogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
