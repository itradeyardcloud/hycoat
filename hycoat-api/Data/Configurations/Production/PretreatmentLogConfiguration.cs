using HycoatApi.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HycoatApi.Data.Configurations.Production;

public class PretreatmentLogConfiguration : IEntityTypeConfiguration<PretreatmentLog>
{
    public void Configure(EntityTypeBuilder<PretreatmentLog> builder)
    {
        builder.Property(e => e.Shift).IsRequired().HasMaxLength(10);
        builder.Property(e => e.EtchTimeMins).HasPrecision(6, 2);
        builder.Property(e => e.Remarks).HasMaxLength(1000);

        builder.HasOne(e => e.ProductionWorkOrder)
            .WithMany(pwo => pwo.PretreatmentLogs)
            .HasForeignKey(e => e.ProductionWorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.OperatorUser)
            .WithMany()
            .HasForeignKey(e => e.OperatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.QASignOffUser)
            .WithMany()
            .HasForeignKey(e => e.QASignOffUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
