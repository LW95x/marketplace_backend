using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class ShoppingCartItemChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCartsItem_Products_ProductId",
                table: "ShoppingCartsItem");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCartsItem_ShoppingCarts_ShoppingCartId",
                table: "ShoppingCartsItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShoppingCartsItem",
                table: "ShoppingCartsItem");

            migrationBuilder.RenameTable(
                name: "ShoppingCartsItem",
                newName: "ShoppingCartItems");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCartsItem_ShoppingCartId",
                table: "ShoppingCartItems",
                newName: "IX_ShoppingCartItems_ShoppingCartId");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCartsItem_ProductId",
                table: "ShoppingCartItems",
                newName: "IX_ShoppingCartItems_ProductId");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ShoppingCartItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShoppingCartItems",
                table: "ShoppingCartItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCartItems_Products_ProductId",
                table: "ShoppingCartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCartItems_ShoppingCarts_ShoppingCartId",
                table: "ShoppingCartItems",
                column: "ShoppingCartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCartItems_Products_ProductId",
                table: "ShoppingCartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCartItems_ShoppingCarts_ShoppingCartId",
                table: "ShoppingCartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShoppingCartItems",
                table: "ShoppingCartItems");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ShoppingCartItems");

            migrationBuilder.RenameTable(
                name: "ShoppingCartItems",
                newName: "ShoppingCartsItem");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCartItems_ShoppingCartId",
                table: "ShoppingCartsItem",
                newName: "IX_ShoppingCartsItem_ShoppingCartId");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCartItems_ProductId",
                table: "ShoppingCartsItem",
                newName: "IX_ShoppingCartsItem_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShoppingCartsItem",
                table: "ShoppingCartsItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCartsItem_Products_ProductId",
                table: "ShoppingCartsItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCartsItem_ShoppingCarts_ShoppingCartId",
                table: "ShoppingCartsItem",
                column: "ShoppingCartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
