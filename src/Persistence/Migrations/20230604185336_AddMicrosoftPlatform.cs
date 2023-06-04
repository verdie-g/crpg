using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddMicrosoftPlatform : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:platform", "steam,epic_games,microsoft")
            .OldAnnotation("Npgsql:Enum:platform", "steam,epic_games");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:platform", "steam,epic_games")
            .OldAnnotation("Npgsql:Enum:platform", "steam,epic_games,microsoft");
    }
}
