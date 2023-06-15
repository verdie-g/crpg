using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddGenerationToUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "generation",
            table: "users",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "generation",
            table: "users");
    }
}
