using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddServerConfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()

            .Annotation("Npgsql:Enum:role", "user,moderator,game_admin,admin")
            .OldAnnotation("Npgsql:Enum:role", "user,moderator,admin");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:role", "user,moderator,admin")
            .OldAnnotation("Npgsql:Enum:role", "user,moderator,game_admin,admin");
    }
}
