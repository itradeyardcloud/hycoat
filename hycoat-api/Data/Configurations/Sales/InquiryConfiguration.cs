using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Sales;

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.Property(e => e.InquiryNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.InquiryNumber).IsUnique();

        builder.Property(e => e.ProjectName).HasMaxLength(300);
        builder.Property(e => e.Source).IsRequired().HasMaxLength(30);
        builder.Property(e => e.ContactPerson).HasMaxLength(200);
        builder.Property(e => e.ContactEmail).HasMaxLength(200);
        builder.Property(e => e.ContactPhone).HasMaxLength(20);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(30);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Inquiries)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProcessType)
            .WithMany()
            .HasForeignKey(e => e.ProcessTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AssignedToUser)
            .WithMany()
            .HasForeignKey(e => e.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
