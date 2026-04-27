using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerceMotoRepuestos.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "Email", "FullName", "Password", "Type" },
                values: new object[] { -1, "superadmin@admin", "Super Admin", "AQAAAAIAAYagAAAAEFyeC2cws9pGHGRNbNLuJIEL0abroIXyMieFsiiI9K2yBi6CPSPI8FRiNwlW/yuu3Q==", "SuperAdmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "UserId",
                keyValue: -1);
        }
    }
}
