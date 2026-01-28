using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImagesShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedWishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Images_ImageId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ImageId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Images");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "Images",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_ImageId",
                table: "Images",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Images_ImageId",
                table: "Images",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }
    }
}
