using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
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
                    goob_currency_store_inventory_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prototype = table.Column<string>(type: "text", nullable: false),
                    immediate = table.Column<bool>(type: "boolean", nullable: false),
                    uses_left = table.Column<int>(type: "integer", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: true)
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
