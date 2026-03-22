using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Quality;

public class InProcessTestResultConfiguration : IEntityTypeConfiguration<InProcessTestResult>
{
    public void Configure(EntityTypeBuilder<InProcessTestResult> builder)
    {
        builder.Property(e => e.TestType).IsRequired().HasMaxLength(30);
        builder.Property(e => e.InstrumentName).HasMaxLength(100);
        builder.Property(e => e.InstrumentMake).HasMaxLength(100);
        builder.Property(e => e.InstrumentModel).HasMaxLength(100);
        builder.Property(e => e.ReferenceStandard).HasMaxLength(100);
        builder.Property(e => e.StandardLimit).HasMaxLength(100);
        builder.Property(e => e.Result).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Remarks).HasMaxLength(500);

        builder.HasOne(e => e.InProcessInspection)
            .WithMany(ipi => ipi.TestResults)
            .HasForeignKey(e => e.InProcessInspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
