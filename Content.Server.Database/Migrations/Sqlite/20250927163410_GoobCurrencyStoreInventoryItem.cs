using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class GoobCurrencyStoreInventoryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "goob_currency_store_inventory",
                columns: table => new
                {
                    goob_currency_store_inventory_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    player_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    prototype = table.Column<string>(type: "TEXT", nullable: false),
                    immediate = table.Column<bool>(type: "INTEGER", nullable: false),
                    uses_left = table.Column<int>(type: "INTEGER", nullable: false),
                    player_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goob_currency_store_inventory", x => x.goob_currency_store_inventory_id);
                    table.ForeignKey(
                        name: "FK_goob_currency_store_inventory_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "player_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_goob_currency_store_inventory_player_id",
                table: "goob_currency_store_inventory",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_goob_currency_store_inventory_player_user_id",
                table: "goob_currency_store_inventory",
                column: "player_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goob_currency_store_inventory");
        }
    }
}
