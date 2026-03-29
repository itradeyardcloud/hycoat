using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HycoatApi.Migrations
{
    /// <inheritdoc />
    public partial class AddYieldReportsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YieldReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProductionUnitId = table.Column<int>(type: "int", nullable: false),
                    ProductionSFT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RejectionSFT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ElectricityOpeningKwh = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ElectricityClosingKwh = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ElectricityRatePerKwh = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OvenGasOpeningReading = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    OvenGasClosingReading = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    OvenGasRatePerUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PowderUsedKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PowderRatePerKg = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ManpowerCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SellingPricePerSFT = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YieldReports_ProductionUnits_ProductionUnitId",
                        column: x => x.ProductionUnitId,
                        principalTable: "ProductionUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin",
                column: "ConcurrencyStamp",
                value: "b4b46d4f-0b0f-48be-a52f-2c61b1d3e958");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-leader",
                column: "ConcurrencyStamp",
                value: "b2ef22eb-d7a1-4d8e-bd01-695f85b3791e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-user",
                column: "ConcurrencyStamp",
                value: "1cc3ac77-33d7-4747-9c52-c28a4532a326");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "e6e72047-a46d-4e3d-a4a6-f70489188fd8", "AQAAAAIAAYagAAAAEPmKwK9k3mOV1YB8zPDXP4yvChyQa28zHI4hXoeDF1UajS3ySQ/c1Q41j63fjp2eKw==" });

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_Date_Shift_ProductionUnitId",
                table: "YieldReports",
                columns: new[] { "Date", "Shift", "ProductionUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_ProductionUnitId",
                table: "YieldReports",
                column: "ProductionUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YieldReports");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin",
                column: "ConcurrencyStamp",
                value: "932695cf-b3ef-4cbe-8b95-3ea14ee48fd5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-leader",
                column: "ConcurrencyStamp",
                value: "dfa8c57f-bfd0-4f66-875f-b77925eb99f7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-user",
                column: "ConcurrencyStamp",
                value: "2eec71ae-27c3-438b-9694-c7d400fc5864");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "fd7f422d-d7ff-402b-bfc9-914737cdeede", "AQAAAAIAAYagAAAAEKlLWV15Bqluud7hl3g+P5fxIu3XdyQ0xtwW9mU2+Bc+zX4pwgd1CFNoVFIK44nDJw==" });
        }
    }
}
