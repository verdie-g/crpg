using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class RemoveRegionFromParty : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "region",
            table: "parties");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Region>(
            name: "region",
            table: "parties",
            type: "region",
            nullable: false,
            defaultValue: Region.Eu);
    }
}
