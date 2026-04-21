using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerceMotoRepuestos.Migrations
{
    /// <inheritdoc />
    public partial class Add_AppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSetting",
                columns: table => new
                {
                    AppSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSetting", x => x.AppSettingId);
                });

            migrationBuilder.InsertData(
                table: "AppSetting",
                columns: new[] { "AppSettingId", "Key", "Value" },
                values: new object[] { 1, "LowStockThreshold", "5" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSetting_Key",
                table: "AppSetting",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSetting");
        }
    }
}
