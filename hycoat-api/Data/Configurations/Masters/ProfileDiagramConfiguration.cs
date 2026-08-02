using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Masters;

public class ProfileDiagramConfiguration : IEntityTypeConfiguration<ProfileDiagram>
{
    public void Configure(EntityTypeBuilder<ProfileDiagram> builder)
    {
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.Family).HasMaxLength(20);
        builder.Property(e => e.Series).HasMaxLength(20);
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.CategoryLabel).HasMaxLength(200);
        builder.Property(e => e.System).HasMaxLength(200);
        builder.Property(e => e.ImageUrl).HasMaxLength(1000);
        builder.Property(e => e.Notes).HasMaxLength(1000);
    }
}
