using HycoatApi.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Common;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.Endpoint).IsRequired().HasMaxLength(500);
        builder.Property(e => e.P256dh).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Auth).IsRequired().HasMaxLength(500);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId);
    }
}
