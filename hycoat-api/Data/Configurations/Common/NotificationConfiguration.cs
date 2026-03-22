using HycoatApi.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Common;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Type).IsRequired().HasMaxLength(50);
        builder.Property(e => e.EntityType).HasMaxLength(100);
        builder.Property(e => e.RecipientUserId).IsRequired();

        builder.HasOne(e => e.Recipient)
            .WithMany()
            .HasForeignKey(e => e.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.RecipientUserId);
    }
}
