using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Masters;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(300);
        builder.Property(e => e.ShortName).HasMaxLength(50);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.State).HasMaxLength(100);
        builder.Property(e => e.Pincode).HasMaxLength(10);
        builder.Property(e => e.GSTIN).HasMaxLength(15);
        builder.Property(e => e.ContactPerson).HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.Notes).HasMaxLength(1000);
    }
}
