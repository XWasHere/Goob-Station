using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class GoobPlayerInventoryItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goob_currency_store_inventory");

            migrationBuilder.DropTable(
                name: "goob_currency_store_permanent_items");

            migrationBuilder.DropTable(
                name: "goob_currency_store_vouchers");

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

            migrationBuilder.CreateTable(
                name: "goob_currency_store_inventory",
                columns: table => new
                {
                    goob_currency_store_inventory_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    immediate = table.Column<bool>(type: "boolean", nullable: false),
                    prototype = table.Column<string>(type: "text", nullable: false),
                    uses_left = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goob_currency_store_inventory", x => x.goob_currency_store_inventory_id);
                    table.ForeignKey(
                        name: "FK_goob_currency_store_inventory_player_player_user_id",
                        column: x => x.player_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "goob_currency_store_permanent_items",
                columns: table => new
                {
                    player_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prototype = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "goob_currency_store_vouchers",
                columns: table => new
                {
                    goob_currency_store_vouchers_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prototype = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goob_currency_store_vouchers", x => x.goob_currency_store_vouchers_id);
                    table.ForeignKey(
                        name: "FK_goob_currency_store_vouchers_player_player_user_id",
                        column: x => x.player_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_goob_currency_store_inventory_player_user_id",
                table: "goob_currency_store_inventory",
                column: "player_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_goob_currency_store_vouchers_player_user_id",
                table: "goob_currency_store_vouchers",
                column: "player_user_id");
        }
    }
}
