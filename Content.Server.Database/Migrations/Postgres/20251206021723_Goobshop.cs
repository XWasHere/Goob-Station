using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class Goobshop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "goob_player_store_items",
                columns: table => new
                {
                    goob_player_store_items_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prototype = table.Column<string>(type: "text", nullable: false),
                    item_type = table.Column<int>(type: "integer", nullable: false),
                    immediate = table.Column<bool>(type: "boolean", nullable: true),
                    uses_left = table.Column<int>(type: "integer", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "goob_store_item_data",
                columns: table => new
                {
                    goob_store_item_data_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prototype = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goob_store_item_data", x => x.goob_store_item_data_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_goob_player_store_items_player_id",
                table: "goob_player_store_items",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_goob_player_store_items_player_id_prototype",
                table: "goob_player_store_items",
                columns: new[] { "player_id", "prototype" });

            migrationBuilder.CreateIndex(
                name: "IX_goob_store_item_data_prototype",
                table: "goob_store_item_data",
                column: "prototype");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goob_player_store_items");

            migrationBuilder.DropTable(
                name: "goob_store_item_data");
        }
    }
}
