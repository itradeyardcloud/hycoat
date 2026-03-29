using HycoatApi.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Common;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(10);
        builder.Property(e => e.UserName).HasMaxLength(100);
        builder.Property(e => e.IpAddress).HasMaxLength(45);

        builder.HasIndex(e => new { e.EntityName, e.EntityId });
        builder.HasIndex(e => e.Timestamp);
    }
}
