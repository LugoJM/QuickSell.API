using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickSell.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnnecesaryField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "SaleItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "SaleItems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
