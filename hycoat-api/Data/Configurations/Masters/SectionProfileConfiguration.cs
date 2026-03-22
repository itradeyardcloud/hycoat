using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Masters;

public class SectionProfileConfiguration : IEntityTypeConfiguration<SectionProfile>
{
    public void Configure(EntityTypeBuilder<SectionProfile> builder)
    {
        builder.Property(e => e.SectionNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.SectionNumber).IsUnique();

        builder.Property(e => e.Type).HasMaxLength(100);
        builder.Property(e => e.PerimeterMM).HasPrecision(10, 2);
        builder.Property(e => e.WeightPerMeter).HasPrecision(10, 4);
        builder.Property(e => e.HeightMM).HasPrecision(10, 2);
        builder.Property(e => e.WidthMM).HasPrecision(10, 2);
        builder.Property(e => e.ThicknessMM).HasPrecision(6, 2);
        builder.Property(e => e.DrawingFileUrl).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(1000);
    }
}
