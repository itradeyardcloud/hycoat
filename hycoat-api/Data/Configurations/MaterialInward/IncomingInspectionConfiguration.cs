using HycoatApi.Models.MaterialInward;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.MaterialInward;

public class IncomingInspectionConfiguration : IEntityTypeConfiguration<IncomingInspection>
{
    public void Configure(EntityTypeBuilder<IncomingInspection> builder)
    {
        builder.Property(e => e.InspectionNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.InspectionNumber).IsUnique();

        builder.Property(e => e.OverallStatus).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Remarks).HasMaxLength(2000);

        builder.HasOne(e => e.MaterialInward)
            .WithMany(mi => mi.IncomingInspections)
            .HasForeignKey(e => e.MaterialInwardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InspectedByUser)
            .WithMany()
            .HasForeignKey(e => e.InspectedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
