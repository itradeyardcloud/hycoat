using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HycoatApi.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelForAzure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
