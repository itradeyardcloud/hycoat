using HycoatApi.Models.Dispatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Dispatch;

public class DCLineItemConfiguration : IEntityTypeConfiguration<DCLineItem>
{
    public void Configure(EntityTypeBuilder<DCLineItem> builder)
    {
        builder.Property(e => e.LengthMM).HasPrecision(10, 2);
        builder.Property(e => e.CustomerDCRef).HasMaxLength(50);
        builder.Property(e => e.Remarks).HasMaxLength(300);

        builder.HasOne(e => e.DeliveryChallan)
            .WithMany(dc => dc.LineItems)
            .HasForeignKey(e => e.DeliveryChallanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SectionProfile)
            .WithMany()
            .HasForeignKey(e => e.SectionProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
