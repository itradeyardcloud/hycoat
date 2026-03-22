using HycoatApi.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Common;

public class FileAttachmentConfiguration : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(EntityTypeBuilder<FileAttachment> builder)
    {
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.StoredPath).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ContentType).HasMaxLength(100);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.UploadedBy).HasMaxLength(100);

        builder.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}
