using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImagesShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserTransactionOrderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "UserTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "UserTransactions",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "UserTransactions");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceId",
                table: "UserTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
