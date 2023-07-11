using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class UserRegionNonNullable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Region>(
            name: "region",
            table: "users",
            type: "region",
            nullable: false,
            defaultValue: Region.Eu,
            oldClrType: typeof(Region),
            oldType: "region",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Region>(
            name: "region",
            table: "users",
            type: "region",
            nullable: true,
            oldClrType: typeof(Region),
            oldType: "region");
    }
}
