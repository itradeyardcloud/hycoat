using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Quality;

public class TestCertificateConfiguration : IEntityTypeConfiguration<TestCertificate>
{
    public void Configure(EntityTypeBuilder<TestCertificate> builder)
    {
        builder.Property(e => e.CertificateNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.CertificateNumber).IsUnique();

        builder.Property(e => e.ProductCode).HasMaxLength(50);
        builder.Property(e => e.ProjectName).HasMaxLength(300);
        builder.Property(e => e.Warranty).HasMaxLength(20);
        builder.Property(e => e.SubstrateResult).HasMaxLength(100);
        builder.Property(e => e.BakingTempResult).HasMaxLength(50);
        builder.Property(e => e.BakingTimeResult).HasMaxLength(50);
        builder.Property(e => e.ColorResult).HasMaxLength(50);
        builder.Property(e => e.DFTResult).HasMaxLength(50);
        builder.Property(e => e.MEKResult).HasMaxLength(50);
        builder.Property(e => e.CrossCutResult).HasMaxLength(50);
        builder.Property(e => e.ConicalMandrelResult).HasMaxLength(50);
        builder.Property(e => e.BoilingWaterResult).HasMaxLength(50);
        builder.Property(e => e.FileUrl).HasMaxLength(500);

        builder.HasOne(e => e.FinalInspection)
            .WithOne(fi => fi.TestCertificate)
            .HasForeignKey<TestCertificate>(e => e.FinalInspectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WorkOrder)
            .WithMany()
            .HasForeignKey(e => e.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
