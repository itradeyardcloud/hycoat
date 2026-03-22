using HycoatApi.Models.MaterialInward;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.MaterialInward;

public class MaterialInwardLineConfiguration : IEntityTypeConfiguration<MaterialInwardLine>
{
    public void Configure(EntityTypeBuilder<MaterialInwardLine> builder)
    {
        builder.Property(e => e.LengthMM).HasPrecision(10, 2);
        builder.Property(e => e.WeightKg).HasPrecision(12, 2);
        builder.Property(e => e.Remarks).HasMaxLength(500);

        builder.HasOne(e => e.MaterialInward)
            .WithMany(mi => mi.Lines)
            .HasForeignKey(e => e.MaterialInwardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SectionProfile)
            .WithMany()
            .HasForeignKey(e => e.SectionProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
