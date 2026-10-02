using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerceMotoRepuestos.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSearchText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "Product",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "Product");
        }
    }
}
