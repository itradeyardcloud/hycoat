using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Masters;

public class ProcessTypeConfiguration : IEntityTypeConfiguration<ProcessType>
{
    public void Configure(EntityTypeBuilder<ProcessType> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.DefaultRatePerSFT).HasPrecision(10, 2);
        builder.Property(e => e.Description).HasMaxLength(500);
    }
}
