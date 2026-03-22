using HycoatApi.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Common;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(20);
        builder.Property(e => e.UserName).HasMaxLength(100);

        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.Timestamp);
    }
}
