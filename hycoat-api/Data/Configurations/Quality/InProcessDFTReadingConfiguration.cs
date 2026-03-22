using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Quality;

public class InProcessDFTReadingConfiguration : IEntityTypeConfiguration<InProcessDFTReading>
{
    public void Configure(EntityTypeBuilder<InProcessDFTReading> builder)
    {
        builder.Property(e => e.S1).HasPrecision(6, 2);
        builder.Property(e => e.S2).HasPrecision(6, 2);
        builder.Property(e => e.S3).HasPrecision(6, 2);
        builder.Property(e => e.S4).HasPrecision(6, 2);
        builder.Property(e => e.S5).HasPrecision(6, 2);
        builder.Property(e => e.S6).HasPrecision(6, 2);
        builder.Property(e => e.S7).HasPrecision(6, 2);
        builder.Property(e => e.S8).HasPrecision(6, 2);
        builder.Property(e => e.S9).HasPrecision(6, 2);
        builder.Property(e => e.S10).HasPrecision(6, 2);
        builder.Property(e => e.MinReading).HasPrecision(6, 2);
        builder.Property(e => e.MaxReading).HasPrecision(6, 2);
        builder.Property(e => e.AvgReading).HasPrecision(6, 2);

        builder.HasOne(e => e.InProcessInspection)
            .WithMany(ipi => ipi.DFTReadings)
            .HasForeignKey(e => e.InProcessInspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SectionProfile)
            .WithMany()
            .HasForeignKey(e => e.SectionProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
