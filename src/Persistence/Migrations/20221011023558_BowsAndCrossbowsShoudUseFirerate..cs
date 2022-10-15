using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

public partial class BowsAndCrossbowsShoudUseFirerate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "primary_weapon_item_usage",
            table: "items",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "secondary_weapon_item_usage",
            table: "items",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "tertiary_weapon_item_usage",
            table: "items",
            type: "text",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "primary_weapon_item_usage",
            table: "items");

        migrationBuilder.DropColumn(
            name: "secondary_weapon_item_usage",
            table: "items");

        migrationBuilder.DropColumn(
            name: "tertiary_weapon_item_usage",
            table: "items");
    }
}
