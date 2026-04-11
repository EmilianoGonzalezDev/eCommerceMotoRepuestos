using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerceMotoRepuestos.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE [Order] SET [PaymentType] = 1, [Status] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Order");
        }
    }
}
