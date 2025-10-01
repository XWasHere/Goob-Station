using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class GoobPlayerInventoryItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "goob_player_store_items",
                columns: table => new
                {
                    goob_player_store_items_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    player_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    prototype = table.Column<string>(type: "TEXT", nullable: false),
                    item_type = table.Column<int>(type: "INTEGER", nullable: false),
                    immediate = table.Column<bool>(type: "INTEGER", nullable: true),
                    uses_left = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goob_player_store_items", x => x.goob_player_store_items_id);
                    table.ForeignKey(
                        name: "FK_goob_player_store_items_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_goob_player_store_items_player_id",
                table: "goob_player_store_items",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_goob_player_store_items_player_id_prototype",
                table: "goob_player_store_items",
                columns: new[] { "player_id", "prototype" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goob_player_store_items");
        }
    }
}
