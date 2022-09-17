using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

public partial class AddBannerToClans : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "color",
            table: "clans");

        migrationBuilder.AddColumn<long>(
            name: "primary_color",
            table: "clans",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<long>(
            name: "secondary_color",
            table: "clans",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<string>(
            name: "banner_key",
            table: "clans",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "primary_color",
            table: "clans");

        migrationBuilder.DropColumn(
            name: "secondary_color",
            table: "clans");

        migrationBuilder.DropColumn(
            name: "banner_key",
            table: "clans");

        migrationBuilder.AddColumn<string>(
            name: "color",
            table: "clans",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }
}
