using HycoatApi.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Production;

public class ProductionPhotoConfiguration : IEntityTypeConfiguration<ProductionPhoto>
{
    public void Configure(EntityTypeBuilder<ProductionPhoto> builder)
    {
        builder.Property(e => e.PhotoUrl).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Description).HasMaxLength(200);

        builder.HasOne(e => e.ProductionLog)
            .WithMany(pl => pl.Photos)
            .HasForeignKey(e => e.ProductionLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.UploadedByUser)
            .WithMany()
            .HasForeignKey(e => e.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
