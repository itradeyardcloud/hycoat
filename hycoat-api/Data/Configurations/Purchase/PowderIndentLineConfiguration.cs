using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Purchase;

public class PowderIndentLineConfiguration : IEntityTypeConfiguration<PowderIndentLine>
{
    public void Configure(EntityTypeBuilder<PowderIndentLine> builder)
    {
        builder.Property(e => e.RequiredQtyKg).HasPrecision(10, 2);

        builder.HasOne(e => e.PowderIndent)
            .WithMany(pi => pi.Lines)
            .HasForeignKey(e => e.PowderIndentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PowderColor)
            .WithMany()
            .HasForeignKey(e => e.PowderColorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
