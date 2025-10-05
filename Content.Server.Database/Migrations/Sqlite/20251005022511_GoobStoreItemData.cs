using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class GoobStoreItemData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "goob_store_item_data",
                columns: table => new
                {
                    goob_store_item_data_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    prototype = table.Column<string>(type: "TEXT", nullable: false),
                    price = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goob_store_item_data", x => x.goob_store_item_data_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_goob_store_item_data_prototype",
                table: "goob_store_item_data",
                column: "prototype");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goob_store_item_data");
        }
    }
}
