using HycoatApi.Models.MaterialInward;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.MaterialInward;

public class IncomingInspectionLineConfiguration : IEntityTypeConfiguration<IncomingInspectionLine>
{
    public void Configure(EntityTypeBuilder<IncomingInspectionLine> builder)
    {
        builder.Property(e => e.BuffingCharge).HasPrecision(10, 2);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Remarks).HasMaxLength(500);

        builder.HasOne(e => e.IncomingInspection)
            .WithMany(ii => ii.Lines)
            .HasForeignKey(e => e.IncomingInspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.MaterialInwardLine)
            .WithMany()
            .HasForeignKey(e => e.MaterialInwardLineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
