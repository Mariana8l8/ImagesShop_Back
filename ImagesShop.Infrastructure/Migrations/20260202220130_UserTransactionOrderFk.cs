using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImagesShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserTransactionOrderFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserTransactions_OrderId",
                table: "UserTransactions",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTransactions_Orders_OrderId",
                table: "UserTransactions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTransactions_Orders_OrderId",
                table: "UserTransactions");

            migrationBuilder.DropIndex(
                name: "IX_UserTransactions_OrderId",
                table: "UserTransactions");
        }
    }
}
