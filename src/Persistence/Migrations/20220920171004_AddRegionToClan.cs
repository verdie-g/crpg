using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

public partial class AddRegionToClan : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Region>(
            name: "region",
            table: "clans",
            type: "region",
            nullable: false,
            defaultValue: Region.Europe);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "region",
            table: "clans");
    }
}
