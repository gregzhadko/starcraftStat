using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Starcraft.Stat.Migrations
{
    public partial class DateOnlyToDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Games",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "Games",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
