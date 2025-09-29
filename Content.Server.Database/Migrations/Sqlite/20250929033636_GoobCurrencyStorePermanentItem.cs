using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class GoobCurrencyStorePermanentItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goob_currency_store_inventory_player_player_id",
                table: "goob_currency_store_inventory");

            migrationBuilder.DropIndex(
                name: "IX_goob_currency_store_inventory_player_id",
                table: "goob_currency_store_inventory");

            migrationBuilder.DropColumn(
                name: "player_id",
                table: "goob_currency_store_inventory");

            migrationBuilder.CreateTable(
                name: "goob_currency_store_permanent_items",
                columns: table => new
                {
                    player_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    prototype = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goob_currency_store_permanent_items", x => new { x.player_user_id, x.prototype });
                    table.ForeignKey(
                        name: "FK_goob_currency_store_permanent_items_player_player_user_id",
                        column: x => x.player_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_goob_currency_store_inventory_player_player_user_id",
                table: "goob_currency_store_inventory",
                column: "player_user_id",
                principalTable: "player",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goob_currency_store_inventory_player_player_user_id",
                table: "goob_currency_store_inventory");

            migrationBuilder.DropTable(
                name: "goob_currency_store_permanent_items");

            migrationBuilder.AddColumn<int>(
                name: "player_id",
                table: "goob_currency_store_inventory",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_goob_currency_store_inventory_player_id",
                table: "goob_currency_store_inventory",
                column: "player_id");

            migrationBuilder.AddForeignKey(
                name: "FK_goob_currency_store_inventory_player_player_id",
                table: "goob_currency_store_inventory",
                column: "player_id",
                principalTable: "player",
                principalColumn: "player_id");
        }
    }
}
