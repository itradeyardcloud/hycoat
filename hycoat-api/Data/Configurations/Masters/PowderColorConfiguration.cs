using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Masters;

public class PowderColorConfiguration : IEntityTypeConfiguration<PowderColor>
{
    public void Configure(EntityTypeBuilder<PowderColor> builder)
    {
        builder.Property(e => e.PowderCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.ColorName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.RALCode).HasMaxLength(20);
        builder.Property(e => e.Make).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.Vendor)
            .WithMany(v => v.PowderColors)
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
