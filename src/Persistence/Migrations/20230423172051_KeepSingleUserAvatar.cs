using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class KeepSingleUserAvatar : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "avatar_small",
            table: "users");

        migrationBuilder.DropColumn(
            name: "avatar_medium",
            table: "users");

        migrationBuilder.RenameColumn(
            name: "avatar_full",
            table: "users",
            newName: "avatar");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "avatar",
            table: "users",
            newName: "avatar_full");

        migrationBuilder.AddColumn<string>(
            name: "avatar_medium",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "avatar_small",
            table: "users",
            type: "text",
            nullable: true);
    }
}
