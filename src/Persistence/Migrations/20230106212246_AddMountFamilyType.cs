using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddMountFamilyType : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "armor_family_type",
            table: "items",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "mount_family_type",
            table: "items",
            type: "integer",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "armor_family_type",
            table: "items");

        migrationBuilder.DropColumn(
            name: "mount_family_type",
            table: "items");
    }
}
