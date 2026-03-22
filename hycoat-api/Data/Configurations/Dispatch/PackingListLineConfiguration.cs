using HycoatApi.Models.Dispatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Dispatch;

public class PackingListLineConfiguration : IEntityTypeConfiguration<PackingListLine>
{
    public void Configure(EntityTypeBuilder<PackingListLine> builder)
    {
        builder.Property(e => e.LengthMM).HasPrecision(10, 2);
        builder.Property(e => e.Remarks).HasMaxLength(300);

        builder.HasOne(e => e.PackingList)
            .WithMany(pl => pl.Lines)
            .HasForeignKey(e => e.PackingListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SectionProfile)
            .WithMany()
            .HasForeignKey(e => e.SectionProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
