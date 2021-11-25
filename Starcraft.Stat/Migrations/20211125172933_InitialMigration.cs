using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Starcraft.Stat.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Race",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Race", x => x.Name);
                });

            migrationBuilder.InsertData(
                table: "Race",
                column: "Name",
                values: new object[]
                {
                    "Protoss",
                    "Terran",
                    "Zerg"
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Race");
        }
    }
}
