using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HycoatApi.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationAuditAndFileEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    GSTIN = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultRatePerSFT = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TankLengthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TankWidthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TankHeightMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    BucketLengthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    BucketWidthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    BucketHeightMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ConveyorLengthMtrs = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SectionProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PerimeterMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    WeightPerMeter = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    HeightMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    WidthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ThicknessMM = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    DrawingFileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    VendorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GSTIN = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    P256dh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Auth = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InquiryNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProcessTypeId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquiries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inquiries_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inquiries_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inquiries_ProcessTypes_ProcessTypeId",
                        column: x => x.ProcessTypeId,
                        principalTable: "ProcessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PowderColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PowderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RALCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Make = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VendorId = table.Column<int>(type: "int", nullable: true),
                    WarrantyYears = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowderColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowderColors_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InquiryId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ValidityDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreparedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotations_AspNetUsers_PreparedByUserId",
                        column: x => x.PreparedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotations_Inquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalTable: "Inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PowderStocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PowderColorId = table.Column<int>(type: "int", nullable: false),
                    CurrentStockKg = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    ReorderLevelKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowderStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowderStocks_PowderColors_PowderColorId",
                        column: x => x.PowderColorId,
                        principalTable: "PowderColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProformaInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PINumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    QuotationId = table.Column<int>(type: "int", nullable: true),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomerGSTIN = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TransportCharges = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    IGSTRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    IsInterState = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PreparedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProformaInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProformaInvoices_AspNetUsers_PreparedByUserId",
                        column: x => x.PreparedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProformaInvoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProformaInvoices_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    ProcessTypeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RatePerSFT = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    WarrantyYears = table.Column<int>(type: "int", nullable: true),
                    MicronRange = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationLineItems_ProcessTypes_ProcessTypeId",
                        column: x => x.ProcessTypeId,
                        principalTable: "ProcessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationLineItems_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PILineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProformaInvoiceId = table.Column<int>(type: "int", nullable: false),
                    SectionProfileId = table.Column<int>(type: "int", nullable: false),
                    SectionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LengthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PerimeterMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AreaSqMtr = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    AreaSFT = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    RatePerSFT = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PILineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PILineItems_ProformaInvoices_ProformaInvoiceId",
                        column: x => x.ProformaInvoiceId,
                        principalTable: "ProformaInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PILineItems_SectionProfiles_SectionProfileId",
                        column: x => x.SectionProfileId,
                        principalTable: "SectionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WONumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ProformaInvoiceId = table.Column<int>(type: "int", nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ProcessTypeId = table.Column<int>(type: "int", nullable: false),
                    PowderColorId = table.Column<int>(type: "int", nullable: true),
                    DispatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_PowderColors_PowderColorId",
                        column: x => x.PowderColorId,
                        principalTable: "PowderColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ProcessTypes_ProcessTypeId",
                        column: x => x.ProcessTypeId,
                        principalTable: "ProcessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ProformaInvoices_ProformaInvoiceId",
                        column: x => x.ProformaInvoiceId,
                        principalTable: "ProformaInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryChallans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DCNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomerGSTIN = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    VehicleNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LRNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaterialValueApprox = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryChallans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryChallans_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryChallans_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialInwards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InwardNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerDCNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerDCDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VehicleNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UnloadingLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProcessTypeId = table.Column<int>(type: "int", nullable: true),
                    PowderColorId = table.Column<int>(type: "int", nullable: true),
                    ReceivedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialInwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialInwards_AspNetUsers_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialInwards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialInwards_PowderColors_PowderColorId",
                        column: x => x.PowderColorId,
                        principalTable: "PowderColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialInwards_ProcessTypes_ProcessTypeId",
                        column: x => x.ProcessTypeId,
                        principalTable: "ProcessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialInwards_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionWorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PWONumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ProcessTypeId = table.Column<int>(type: "int", nullable: false),
                    PowderColorId = table.Column<int>(type: "int", nullable: true),
                    ProductionUnitId = table.Column<int>(type: "int", nullable: false),
                    PowderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ColorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreTreatmentTimeHrs = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    PostTreatmentTimeHrs = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    TotalTimeHrs = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    ShiftAllocation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PackingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionWorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_PowderColors_PowderColorId",
                        column: x => x.PowderColorId,
                        principalTable: "PowderColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_ProcessTypes_ProcessTypeId",
                        column: x => x.ProcessTypeId,
                        principalTable: "ProcessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_ProductionUnits_ProductionUnitId",
                        column: x => x.ProductionUnitId,
                        principalTable: "ProductionUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionWorkOrders_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DCLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryChallanId = table.Column<int>(type: "int", nullable: false),
                    SectionProfileId = table.Column<int>(type: "int", nullable: false),
                    LengthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CustomerDCRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DCLineItems_DeliveryChallans_DeliveryChallanId",
                        column: x => x.DeliveryChallanId,
                        principalTable: "DeliveryChallans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DCLineItems_SectionProfiles_SectionProfileId",
                        column: x => x.SectionProfileId,
                        principalTable: "SectionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    DeliveryChallanId = table.Column<int>(type: "int", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomerGSTIN = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    OurGSTIN = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    HSNSACCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TransportCharges = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    IGSTRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    AmountInWords = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsInterState = table.Column<bool>(type: "bit", nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankAccountNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BankIFSC = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_DeliveryChallans_DeliveryChallanId",
                        column: x => x.DeliveryChallanId,
                        principalTable: "DeliveryChallans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncomingInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaterialInwardId = table.Column<int>(type: "int", nullable: false),
                    InspectedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OverallStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingInspections_AspNetUsers_InspectedByUserId",
                        column: x => x.InspectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncomingInspections_MaterialInwards_MaterialInwardId",
                        column: x => x.MaterialInwardId,
                        principalTable: "MaterialInwards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialInwardLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialInwardId = table.Column<int>(type: "int", nullable: false),
                    SectionProfileId = table.Column<int>(type: "int", nullable: false),
                    LengthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    QtyAsPerDC = table.Column<int>(type: "int", nullable: false),
                    QtyReceived = table.Column<int>(type: "int", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    Discrepancy = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialInwardLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialInwardLines_MaterialInwards_MaterialInwardId",
                        column: x => x.MaterialInwardId,
                        principalTable: "MaterialInwards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialInwardLines_SectionProfiles_SectionProfileId",
                        column: x => x.SectionProfileId,
                        principalTable: "SectionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinalInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    LotQuantity = table.Column<int>(type: "int", nullable: false),
                    SampledQuantity = table.Column<int>(type: "int", nullable: false),
                    VisualCheckStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DFTRecheckStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ShadeMatchFinalStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OverallStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InspectorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalInspections_AspNetUsers_InspectorUserId",
                        column: x => x.InspectorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinalInspections_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InProcessInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    InspectorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InProcessInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InProcessInspections_AspNetUsers_InspectorUserId",
                        column: x => x.InspectorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InProcessInspections_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackingLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    PackingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BundleCount = table.Column<int>(type: "int", nullable: true),
                    PreparedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackingLists_AspNetUsers_PreparedByUserId",
                        column: x => x.PreparedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackingLists_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackingLists_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PanelTests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    BoilingWaterResult = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BoilingWaterStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ImpactTestResult = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImpactTestStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ConicalMandrelResult = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConicalMandrelStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PencilHardnessResult = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PencilHardnessStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    InspectorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanelTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PanelTests_AspNetUsers_InspectorUserId",
                        column: x => x.InspectorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PanelTests_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PowderIndents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndentNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: true),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowderIndents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowderIndents_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PowderIndents_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PretreatmentLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    BasketNumber = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EtchTimeMins = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    OperatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    QASignOffUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PretreatmentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PretreatmentLogs_AspNetUsers_OperatorUserId",
                        column: x => x.OperatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PretreatmentLogs_AspNetUsers_QASignOffUserId",
                        column: x => x.QASignOffUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PretreatmentLogs_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ConveyorSpeedMtrPerMin = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    OvenTemperature = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    PowderBatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupervisorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionLogs_AspNetUsers_SupervisorUserId",
                        column: x => x.SupervisorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLogs_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductionUnitId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionSchedules_ProductionUnits_ProductionUnitId",
                        column: x => x.ProductionUnitId,
                        principalTable: "ProductionUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionSchedules_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PWOLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionWorkOrderId = table.Column<int>(type: "int", nullable: false),
                    SectionProfileId = table.Column<int>(type: "int", nullable: false),
                    CustomerDCNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LengthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PerimeterMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    UnitSurfaceAreaSqMtr = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    TotalSurfaceAreaSqft = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PWOLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PWOLineItems_ProductionWorkOrders_ProductionWorkOrderId",
                        column: x => x.ProductionWorkOrderId,
                        principalTable: "ProductionWorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PWOLineItems_SectionProfiles_SectionProfileId",
                        column: x => x.SectionProfileId,
                        principalTable: "SectionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    SectionProfileId = table.Column<int>(type: "int", nullable: false),
                    SectionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DCNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MicronRange = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PerimeterMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    LengthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AreaSFT = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    RatePerSFT = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_SectionProfiles_SectionProfileId",
                        column: x => x.SectionProfileId,
                        principalTable: "SectionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncomingInspectionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncomingInspectionId = table.Column<int>(type: "int", nullable: false),
                    MaterialInwardLineId = table.Column<int>(type: "int", nullable: false),
                    WatermarkOk = table.Column<bool>(type: "bit", nullable: true),
                    ScratchOk = table.Column<bool>(type: "bit", nullable: true),
                    DentOk = table.Column<bool>(type: "bit", nullable: true),
                    DimensionalCheckOk = table.Column<bool>(type: "bit", nullable: true),
                    BuffingRequired = table.Column<bool>(type: "bit", nullable: false),
                    BuffingCharge = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingInspectionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingInspectionLines_IncomingInspections_IncomingInspectionId",
                        column: x => x.IncomingInspectionId,
                        principalTable: "IncomingInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncomingInspectionLines_MaterialInwardLines_MaterialInwardLineId",
                        column: x => x.MaterialInwardLineId,
                        principalTable: "MaterialInwardLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificateNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalInspectionId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LotQuantity = table.Column<int>(type: "int", nullable: false),
                    Warranty = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SubstrateResult = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BakingTempResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BakingTimeResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ColorResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DFTResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MEKResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CrossCutResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConicalMandrelResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BoilingWaterResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCertificates_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestCertificates_FinalInspections_FinalInspectionId",
                        column: x => x.FinalInspectionId,
                        principalTable: "FinalInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestCertificates_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InProcessDFTReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InProcessInspectionId = table.Column<int>(type: "int", nullable: false),
                    SectionProfileId = table.Column<int>(type: "int", nullable: true),
                    S1 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    S2 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    S3 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    S4 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    S5 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    S6 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    S7 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    S8 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    S9 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    S10 = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    MinReading = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    MaxReading = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    AvgReading = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    IsWithinSpec = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InProcessDFTReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InProcessDFTReadings_InProcessInspections_InProcessInspectionId",
                        column: x => x.InProcessInspectionId,
                        principalTable: "InProcessInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InProcessDFTReadings_SectionProfiles_SectionProfileId",
                        column: x => x.SectionProfileId,
                        principalTable: "SectionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InProcessTestResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InProcessInspectionId = table.Column<int>(type: "int", nullable: false),
                    TestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InstrumentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InstrumentMake = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InstrumentModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CalibrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceStandard = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StandardLimit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InProcessTestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InProcessTestResults_InProcessInspections_InProcessInspectionId",
                        column: x => x.InProcessInspectionId,
                        principalTable: "InProcessInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackingListLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackingListId = table.Column<int>(type: "int", nullable: false),
                    SectionProfileId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LengthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    BundleNumber = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingListLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackingListLines_PackingLists_PackingListId",
                        column: x => x.PackingListId,
                        principalTable: "PackingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackingListLines_SectionProfiles_SectionProfileId",
                        column: x => x.SectionProfileId,
                        principalTable: "SectionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PowderIndentLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PowderIndentId = table.Column<int>(type: "int", nullable: false),
                    PowderColorId = table.Column<int>(type: "int", nullable: false),
                    RequiredQtyKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowderIndentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowderIndentLines_PowderColors_PowderColorId",
                        column: x => x.PowderColorId,
                        principalTable: "PowderColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PowderIndentLines_PowderIndents_PowderIndentId",
                        column: x => x.PowderIndentId,
                        principalTable: "PowderIndents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PONumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    PowderIndentId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_PowderIndents_PowderIndentId",
                        column: x => x.PowderIndentId,
                        principalTable: "PowderIndents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PretreatmentTankReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PretreatmentLogId = table.Column<int>(type: "int", nullable: false),
                    TankName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concentration = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: true),
                    Temperature = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    ChemicalAdded = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChemicalQty = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PretreatmentTankReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PretreatmentTankReadings_PretreatmentLogs_PretreatmentLogId",
                        column: x => x.PretreatmentLogId,
                        principalTable: "PretreatmentLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionLogId = table.Column<int>(type: "int", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionPhotos_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionPhotos_ProductionLogs_ProductionLogId",
                        column: x => x.ProductionLogId,
                        principalTable: "ProductionLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionTimeCalcs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PWOLineItemId = table.Column<int>(type: "int", nullable: false),
                    ThicknessMM = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    WidthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    HeightMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    SpecificWeight = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: true),
                    WeightPerMtr = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    TotalWeightKg = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    LoadsRequired = table.Column<int>(type: "int", nullable: true),
                    TotalTimePreTreatMins = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ConveyorSpeedMtrPerMin = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    JigLengthMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    GapBetweenJigsMM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalConveyorDistanceMtrs = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    TotalTimePostTreatMins = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionTimeCalcs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionTimeCalcs_PWOLineItems_PWOLineItemId",
                        column: x => x.PWOLineItemId,
                        principalTable: "PWOLineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceivedNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GRNNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ReceivedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceivedNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceivedNotes_AspNetUsers_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceivedNotes_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "POLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    PowderColorId = table.Column<int>(type: "int", nullable: false),
                    QtyKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    RatePerKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    RequiredByDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_POLineItems_PowderColors_PowderColorId",
                        column: x => x.PowderColorId,
                        principalTable: "PowderColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POLineItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GRNLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoodsReceivedNoteId = table.Column<int>(type: "int", nullable: false),
                    PowderColorId = table.Column<int>(type: "int", nullable: false),
                    QtyReceivedKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    BatchCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MfgDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRNLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GRNLineItems_GoodsReceivedNotes_GoodsReceivedNoteId",
                        column: x => x.GoodsReceivedNoteId,
                        principalTable: "GoodsReceivedNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GRNLineItems_PowderColors_PowderColorId",
                        column: x => x.PowderColorId,
                        principalTable: "PowderColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "role-admin", "8b4920ef-d62d-46be-b87d-9c7463f06c48", "Full system access", "Admin", "ADMIN" },
                    { "role-leader", "19802e43-7127-4893-b255-f046a735e022", "Department leader access", "Leader", "LEADER" },
                    { "role-user", "38d032ef-3409-4235-afef-b7609beae348", "Standard user access", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Department", "Email", "EmailConfirmed", "FullName", "IsActive", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiryTime", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "user-admin", 0, "6f4fec49-dc91-4d3a-89e2-e3695f9650a3", "Admin", "admin@hycoat.com", true, "System Administrator", true, false, null, "ADMIN@HYCOAT.COM", "ADMIN@HYCOAT.COM", "AQAAAAIAAYagAAAAELxNYfLuG2jch0CqwBvxyXfnrvo3Ja0Vpm0QQrDBfQ3FnpClf+fNNu2oXzICiD+HKw==", null, false, null, null, "STATIC-SECURITY-STAMP-FOR-SEED", false, "admin@hycoat.com" });

            migrationBuilder.InsertData(
                table: "ProcessTypes",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DefaultRatePerSFT", "Description", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Powder Coating", null, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Anodizing", null, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Wood Effect", null, null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Chromotizing", null, null },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "PVDF", null, null },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Mill Finish", null, null }
                });

            migrationBuilder.InsertData(
                table: "ProductionUnits",
                columns: new[] { "Id", "BucketHeightMM", "BucketLengthMM", "BucketWidthMM", "ConveyorLengthMtrs", "CreatedAt", "CreatedBy", "IsActive", "IsDeleted", "Name", "TankHeightMM", "TankLengthMM", "TankWidthMM", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, null, null, null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Unit-1", null, null, null, null, null },
                    { 2, null, null, null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Unit-2", null, null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "role-admin", "user-admin" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_DCLineItems_DeliveryChallanId",
                table: "DCLineItems",
                column: "DeliveryChallanId");

            migrationBuilder.CreateIndex(
                name: "IX_DCLineItems_SectionProfileId",
                table: "DCLineItems",
                column: "SectionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_CustomerId",
                table: "DeliveryChallans",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_DCNumber",
                table: "DeliveryChallans",
                column: "DCNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_WorkOrderId",
                table: "DeliveryChallans",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FileAttachments_EntityType_EntityId",
                table: "FileAttachments",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_FinalInspections_InspectionNumber",
                table: "FinalInspections",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalInspections_InspectorUserId",
                table: "FinalInspections",
                column: "InspectorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalInspections_ProductionWorkOrderId",
                table: "FinalInspections",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceivedNotes_GRNNumber",
                table: "GoodsReceivedNotes",
                column: "GRNNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceivedNotes_PurchaseOrderId",
                table: "GoodsReceivedNotes",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceivedNotes_ReceivedByUserId",
                table: "GoodsReceivedNotes",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNLineItems_GoodsReceivedNoteId",
                table: "GRNLineItems",
                column: "GoodsReceivedNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNLineItems_PowderColorId",
                table: "GRNLineItems",
                column: "PowderColorId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspectionLines_IncomingInspectionId",
                table: "IncomingInspectionLines",
                column: "IncomingInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspectionLines_MaterialInwardLineId",
                table: "IncomingInspectionLines",
                column: "MaterialInwardLineId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspections_InspectedByUserId",
                table: "IncomingInspections",
                column: "InspectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspections_InspectionNumber",
                table: "IncomingInspections",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspections_MaterialInwardId",
                table: "IncomingInspections",
                column: "MaterialInwardId");

            migrationBuilder.CreateIndex(
                name: "IX_InProcessDFTReadings_InProcessInspectionId",
                table: "InProcessDFTReadings",
                column: "InProcessInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InProcessDFTReadings_SectionProfileId",
                table: "InProcessDFTReadings",
                column: "SectionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_InProcessInspections_InspectorUserId",
                table: "InProcessInspections",
                column: "InspectorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InProcessInspections_ProductionWorkOrderId",
                table: "InProcessInspections",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InProcessTestResults_InProcessInspectionId",
                table: "InProcessTestResults",
                column: "InProcessInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_AssignedToUserId",
                table: "Inquiries",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_CustomerId",
                table: "Inquiries",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_InquiryNumber",
                table: "Inquiries",
                column: "InquiryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_ProcessTypeId",
                table: "Inquiries",
                column: "ProcessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_InvoiceId",
                table: "InvoiceLineItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_SectionProfileId",
                table: "InvoiceLineItems",
                column: "SectionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DeliveryChallanId",
                table: "Invoices",
                column: "DeliveryChallanId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_WorkOrderId",
                table: "Invoices",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwardLines_MaterialInwardId",
                table: "MaterialInwardLines",
                column: "MaterialInwardId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwardLines_SectionProfileId",
                table: "MaterialInwardLines",
                column: "SectionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwards_CustomerId",
                table: "MaterialInwards",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwards_InwardNumber",
                table: "MaterialInwards",
                column: "InwardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwards_PowderColorId",
                table: "MaterialInwards",
                column: "PowderColorId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwards_ProcessTypeId",
                table: "MaterialInwards",
                column: "ProcessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwards_ReceivedByUserId",
                table: "MaterialInwards",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwards_WorkOrderId",
                table: "MaterialInwards",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId",
                table: "Notifications",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingListLines_PackingListId",
                table: "PackingListLines",
                column: "PackingListId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingListLines_SectionProfileId",
                table: "PackingListLines",
                column: "SectionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingLists_PreparedByUserId",
                table: "PackingLists",
                column: "PreparedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingLists_ProductionWorkOrderId",
                table: "PackingLists",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingLists_WorkOrderId",
                table: "PackingLists",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PanelTests_InspectorUserId",
                table: "PanelTests",
                column: "InspectorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PanelTests_ProductionWorkOrderId",
                table: "PanelTests",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PILineItems_ProformaInvoiceId",
                table: "PILineItems",
                column: "ProformaInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PILineItems_SectionProfileId",
                table: "PILineItems",
                column: "SectionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_POLineItems_PowderColorId",
                table: "POLineItems",
                column: "PowderColorId");

            migrationBuilder.CreateIndex(
                name: "IX_POLineItems_PurchaseOrderId",
                table: "POLineItems",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PowderColors_VendorId",
                table: "PowderColors",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PowderIndentLines_PowderColorId",
                table: "PowderIndentLines",
                column: "PowderColorId");

            migrationBuilder.CreateIndex(
                name: "IX_PowderIndentLines_PowderIndentId",
                table: "PowderIndentLines",
                column: "PowderIndentId");

            migrationBuilder.CreateIndex(
                name: "IX_PowderIndents_IndentNumber",
                table: "PowderIndents",
                column: "IndentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PowderIndents_ProductionWorkOrderId",
                table: "PowderIndents",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PowderIndents_RequestedByUserId",
                table: "PowderIndents",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PowderStocks_PowderColorId",
                table: "PowderStocks",
                column: "PowderColorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PretreatmentLogs_OperatorUserId",
                table: "PretreatmentLogs",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PretreatmentLogs_ProductionWorkOrderId",
                table: "PretreatmentLogs",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PretreatmentLogs_QASignOffUserId",
                table: "PretreatmentLogs",
                column: "QASignOffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PretreatmentTankReadings_PretreatmentLogId",
                table: "PretreatmentTankReadings",
                column: "PretreatmentLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessTypes_Name",
                table: "ProcessTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_ProductionWorkOrderId",
                table: "ProductionLogs",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogs_SupervisorUserId",
                table: "ProductionLogs",
                column: "SupervisorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPhotos_ProductionLogId",
                table: "ProductionPhotos",
                column: "ProductionLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPhotos_UploadedByUserId",
                table: "ProductionPhotos",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_Date_Shift_ProductionUnitId",
                table: "ProductionSchedules",
                columns: new[] { "Date", "Shift", "ProductionUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_ProductionUnitId",
                table: "ProductionSchedules",
                column: "ProductionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_ProductionWorkOrderId",
                table: "ProductionSchedules",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTimeCalcs_PWOLineItemId",
                table: "ProductionTimeCalcs",
                column: "PWOLineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionUnits_Name",
                table: "ProductionUnits",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_CustomerId",
                table: "ProductionWorkOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_PowderColorId",
                table: "ProductionWorkOrders",
                column: "PowderColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_ProcessTypeId",
                table: "ProductionWorkOrders",
                column: "ProcessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_ProductionUnitId",
                table: "ProductionWorkOrders",
                column: "ProductionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_PWONumber",
                table: "ProductionWorkOrders",
                column: "PWONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionWorkOrders_WorkOrderId",
                table: "ProductionWorkOrders",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoices_CustomerId",
                table: "ProformaInvoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoices_PINumber",
                table: "ProformaInvoices",
                column: "PINumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoices_PreparedByUserId",
                table: "ProformaInvoices",
                column: "PreparedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoices_QuotationId",
                table: "ProformaInvoices",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PONumber",
                table: "PurchaseOrders",
                column: "PONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PowderIndentId",
                table: "PurchaseOrders",
                column: "PowderIndentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_VendorId",
                table: "PurchaseOrders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptions_UserId",
                table: "PushSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PWOLineItems_ProductionWorkOrderId",
                table: "PWOLineItems",
                column: "ProductionWorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PWOLineItems_SectionProfileId",
                table: "PWOLineItems",
                column: "SectionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationLineItems_ProcessTypeId",
                table: "QuotationLineItems",
                column: "ProcessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationLineItems_QuotationId",
                table: "QuotationLineItems",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_CustomerId",
                table: "Quotations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_InquiryId",
                table: "Quotations",
                column: "InquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_PreparedByUserId",
                table: "Quotations",
                column: "PreparedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_QuotationNumber",
                table: "Quotations",
                column: "QuotationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionProfiles_SectionNumber",
                table: "SectionProfiles",
                column: "SectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestCertificates_CertificateNumber",
                table: "TestCertificates",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestCertificates_CustomerId",
                table: "TestCertificates",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCertificates_FinalInspectionId",
                table: "TestCertificates",
                column: "FinalInspectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestCertificates_WorkOrderId",
                table: "TestCertificates",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CustomerId",
                table: "WorkOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_PowderColorId",
                table: "WorkOrders",
                column: "PowderColorId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProcessTypeId",
                table: "WorkOrders",
                column: "ProcessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProformaInvoiceId",
                table: "WorkOrders",
                column: "ProformaInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WONumber",
                table: "WorkOrders",
                column: "WONumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DCLineItems");

            migrationBuilder.DropTable(
                name: "FileAttachments");

            migrationBuilder.DropTable(
                name: "GRNLineItems");

            migrationBuilder.DropTable(
                name: "IncomingInspectionLines");

            migrationBuilder.DropTable(
                name: "InProcessDFTReadings");

            migrationBuilder.DropTable(
                name: "InProcessTestResults");

            migrationBuilder.DropTable(
                name: "InvoiceLineItems");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PackingListLines");

            migrationBuilder.DropTable(
                name: "PanelTests");

            migrationBuilder.DropTable(
                name: "PILineItems");

            migrationBuilder.DropTable(
                name: "POLineItems");

            migrationBuilder.DropTable(
                name: "PowderIndentLines");

            migrationBuilder.DropTable(
                name: "PowderStocks");

            migrationBuilder.DropTable(
                name: "PretreatmentTankReadings");

            migrationBuilder.DropTable(
                name: "ProductionPhotos");

            migrationBuilder.DropTable(
                name: "ProductionSchedules");

            migrationBuilder.DropTable(
                name: "ProductionTimeCalcs");

            migrationBuilder.DropTable(
                name: "PushSubscriptions");

            migrationBuilder.DropTable(
                name: "QuotationLineItems");

            migrationBuilder.DropTable(
                name: "TestCertificates");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "GoodsReceivedNotes");

            migrationBuilder.DropTable(
                name: "IncomingInspections");

            migrationBuilder.DropTable(
                name: "MaterialInwardLines");

            migrationBuilder.DropTable(
                name: "InProcessInspections");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "PackingLists");

            migrationBuilder.DropTable(
                name: "PretreatmentLogs");

            migrationBuilder.DropTable(
                name: "ProductionLogs");

            migrationBuilder.DropTable(
                name: "PWOLineItems");

            migrationBuilder.DropTable(
                name: "FinalInspections");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "MaterialInwards");

            migrationBuilder.DropTable(
                name: "DeliveryChallans");

            migrationBuilder.DropTable(
                name: "SectionProfiles");

            migrationBuilder.DropTable(
                name: "PowderIndents");

            migrationBuilder.DropTable(
                name: "ProductionWorkOrders");

            migrationBuilder.DropTable(
                name: "ProductionUnits");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "PowderColors");

            migrationBuilder.DropTable(
                name: "ProformaInvoices");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropTable(
                name: "Inquiries");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "ProcessTypes");
        }
    }
}
