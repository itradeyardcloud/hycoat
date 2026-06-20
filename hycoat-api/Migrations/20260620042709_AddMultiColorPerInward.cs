using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HycoatApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiColorPerInward : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialInwards_PowderColors_PowderColorId",
                table: "MaterialInwards");

            migrationBuilder.DropIndex(
                name: "IX_MaterialInwards_PowderColorId",
                table: "MaterialInwards");

            // Create new child table first so data can be migrated
            migrationBuilder.CreateTable(
                name: "MaterialInwardPowderColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialInwardId = table.Column<int>(type: "int", nullable: false),
                    PowderColorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialInwardPowderColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialInwardPowderColors_MaterialInwards_MaterialInwardId",
                        column: x => x.MaterialInwardId,
                        principalTable: "MaterialInwards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialInwardPowderColors_PowderColors_PowderColorId",
                        column: x => x.PowderColorId,
                        principalTable: "PowderColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwardPowderColors_MaterialInwardId",
                table: "MaterialInwardPowderColors",
                column: "MaterialInwardId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwardPowderColors_PowderColorId",
                table: "MaterialInwardPowderColors",
                column: "PowderColorId");

            // Migrate existing single-color data to the new child table before dropping the old column
            migrationBuilder.Sql(@"
                INSERT INTO MaterialInwardPowderColors (MaterialInwardId, PowderColorId)
                SELECT Id, PowderColorId
                FROM MaterialInwards
                WHERE PowderColorId IS NOT NULL AND IsDeleted = 0
            ");

            // Now safe to drop the old single-color column
            migrationBuilder.DropColumn(
                name: "PowderColorId",
                table: "MaterialInwards");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialInwardPowderColors");

            migrationBuilder.AddColumn<int>(
                name: "PowderColorId",
                table: "MaterialInwards",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin",
                column: "ConcurrencyStamp",
                value: "ad3940de-faa8-47b1-9a0a-619f4cdadbc9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-leader",
                column: "ConcurrencyStamp",
                value: "d534edb1-f9a3-4dad-8dfa-ed012ef6ca74");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-user",
                column: "ConcurrencyStamp",
                value: "8382034b-a945-4938-badc-1cad45971b25");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "1417712c-74cb-4bf3-bffc-b73fd85fd44e", "AQAAAAIAAYagAAAAEJH0FgM1qVWjwxqIhRz3aYtgCix0lhavpCRnh+fNjHp5B/7pphIG2aLRvB1JxRlqag==" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInwards_PowderColorId",
                table: "MaterialInwards",
                column: "PowderColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialInwards_PowderColors_PowderColorId",
                table: "MaterialInwards",
                column: "PowderColorId",
                principalTable: "PowderColors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
