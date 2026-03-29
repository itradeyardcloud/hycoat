using System.Linq.Expressions;
using System.Reflection;
using HycoatApi.Models.Common;
using HycoatApi.Models.Dispatch;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Masters;
using HycoatApi.Models.MaterialInward;
using HycoatApi.Models.Planning;
using HycoatApi.Models.Production;
using HycoatApi.Models.Purchase;
using HycoatApi.Models.Quality;
using HycoatApi.Models.Sales;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Common
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

    // Masters
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<SectionProfile> SectionProfiles => Set<SectionProfile>();
    public DbSet<PowderColor> PowderColors => Set<PowderColor>();
    public DbSet<ProcessType> ProcessTypes => Set<ProcessType>();
    public DbSet<ProductionUnit> ProductionUnits => Set<ProductionUnit>();

    // Sales
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationLineItem> QuotationLineItems => Set<QuotationLineItem>();
    public DbSet<ProformaInvoice> ProformaInvoices => Set<ProformaInvoice>();
    public DbSet<PILineItem> PILineItems => Set<PILineItem>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    // Material Inward
    public DbSet<Models.MaterialInward.MaterialInward> MaterialInwards => Set<Models.MaterialInward.MaterialInward>();
    public DbSet<MaterialInwardLine> MaterialInwardLines => Set<MaterialInwardLine>();
    public DbSet<IncomingInspection> IncomingInspections => Set<IncomingInspection>();
    public DbSet<IncomingInspectionLine> IncomingInspectionLines => Set<IncomingInspectionLine>();

    // Planning
    public DbSet<ProductionWorkOrder> ProductionWorkOrders => Set<ProductionWorkOrder>();
    public DbSet<PWOLineItem> PWOLineItems => Set<PWOLineItem>();
    public DbSet<ProductionSchedule> ProductionSchedules => Set<ProductionSchedule>();
    public DbSet<ProductionTimeCalc> ProductionTimeCalcs => Set<ProductionTimeCalc>();

    // Production
    public DbSet<PretreatmentLog> PretreatmentLogs => Set<PretreatmentLog>();
    public DbSet<PretreatmentTankReading> PretreatmentTankReadings => Set<PretreatmentTankReading>();
    public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();
    public DbSet<ProductionPhoto> ProductionPhotos => Set<ProductionPhoto>();
    public DbSet<YieldReport> YieldReports => Set<YieldReport>();

    // Quality
    public DbSet<InProcessInspection> InProcessInspections => Set<InProcessInspection>();
    public DbSet<InProcessDFTReading> InProcessDFTReadings => Set<InProcessDFTReading>();
    public DbSet<InProcessTestResult> InProcessTestResults => Set<InProcessTestResult>();
    public DbSet<PanelTest> PanelTests => Set<PanelTest>();
    public DbSet<FinalInspection> FinalInspections => Set<FinalInspection>();
    public DbSet<TestCertificate> TestCertificates => Set<TestCertificate>();

    // Dispatch
    public DbSet<PackingList> PackingLists => Set<PackingList>();
    public DbSet<PackingListLine> PackingListLines => Set<PackingListLine>();
    public DbSet<DeliveryChallan> DeliveryChallans => Set<DeliveryChallan>();
    public DbSet<DCLineItem> DCLineItems => Set<DCLineItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();

    // Purchase
    public DbSet<PowderIndent> PowderIndents => Set<PowderIndent>();
    public DbSet<PowderIndentLine> PowderIndentLines => Set<PowderIndentLine>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<POLineItem> POLineItems => Set<POLineItem>();
    public DbSet<GoodsReceivedNote> GoodsReceivedNotes => Set<GoodsReceivedNote>();
    public DbSet<GRNLineItem> GRNLineItems => Set<GRNLineItem>();
    public DbSet<PowderStock> PowderStocks => Set<PowderStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global soft-delete query filter for all BaseEntity-derived types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(property), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        // Seed Roles
        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = "role-admin", Name = "Admin", NormalizedName = "ADMIN", Description = "Full system access" },
            new AppRole { Id = "role-leader", Name = "Leader", NormalizedName = "LEADER", Description = "Department leader access" },
            new AppRole { Id = "role-user", Name = "User", NormalizedName = "USER", Description = "Standard user access" }
        );

        // Seed Admin User
        var adminUserId = "user-admin";
        var hasher = new PasswordHasher<AppUser>();
        var adminUser = new AppUser
        {
            Id = adminUserId,
            UserName = "admin@hycoat.com",
            NormalizedUserName = "ADMIN@HYCOAT.COM",
            Email = "admin@hycoat.com",
            NormalizedEmail = "ADMIN@HYCOAT.COM",
            EmailConfirmed = true,
            FullName = "System Administrator",
            Department = "Admin",
            IsActive = true,
            SecurityStamp = "STATIC-SECURITY-STAMP-FOR-SEED"
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");
        modelBuilder.Entity<AppUser>().HasData(adminUser);

        // Assign Admin role to admin user
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { UserId = adminUserId, RoleId = "role-admin" }
        );

        // Seed Process Types
        modelBuilder.Entity<ProcessType>().HasData(
            new ProcessType { Id = 1, Name = "Powder Coating", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProcessType { Id = 2, Name = "Anodizing", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProcessType { Id = 3, Name = "Wood Effect", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProcessType { Id = 4, Name = "Chromotizing", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProcessType { Id = 5, Name = "PVDF", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProcessType { Id = 6, Name = "Mill Finish", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed Production Units
        modelBuilder.Entity<ProductionUnit>().HasData(
            new ProductionUnit { Id = 1, Name = "Unit-1", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductionUnit { Id = 2, Name = "Unit-2", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
