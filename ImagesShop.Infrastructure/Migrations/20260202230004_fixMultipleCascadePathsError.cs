using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImagesShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixMultipleCascadePathsError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTransactions_Orders_OrderId",
                table: "UserTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTransactions_Orders_OrderId",
                table: "UserTransactions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTransactions_Orders_OrderId",
                table: "UserTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTransactions_Orders_OrderId",
                table: "UserTransactions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
