using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HycoatApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileDiagrams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileDiagrams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Family = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Series = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CategoryLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    System = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WidthPx = table.Column<int>(type: "int", nullable: true),
                    HeightPx = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileDiagrams", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin",
                column: "ConcurrencyStamp",
                value: "4ee3f435-a870-400a-b768-6bf3a585a9c9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-leader",
                column: "ConcurrencyStamp",
                value: "424dbc85-cd75-4283-93c3-aabc58eaa246");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-user",
                column: "ConcurrencyStamp",
                value: "6e1a79f2-dc08-406d-a734-8f897d2eece7");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "ff224ff8-fb0b-43ee-b9d9-fef6ad129a3d", "AQAAAAIAAYagAAAAEGuvccj9HXRDywn38426Ck2FGXR3IkVHPigtz9eKDwznITB0P3w4kXz32OQ6a6rhFw==" });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileDiagrams_Code",
                table: "ProfileDiagrams",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileDiagrams");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin",
                column: "ConcurrencyStamp",
                value: "56603374-f8cf-43d9-814d-b7f0d4ef1a03");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-leader",
                column: "ConcurrencyStamp",
                value: "4ac4ce14-db72-4eb2-8853-a0e1ef2c67f2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-user",
                column: "ConcurrencyStamp",
                value: "18975004-0ae2-4da5-8cd4-c95c77de045f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "43247552-9fe9-429d-b044-42d4087c7cd3", "AQAAAAIAAYagAAAAELRpNsyTnUDrpup0cOmeUwVZDGfSm1fIIxJOSV5YHAVQbYNVyR7BVihSHJkTQTjq+A==" });
        }
    }
}
