using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCharacterLimitations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "character_limitations",
            columns: table => new
            {
                characterid = table.Column<int>(name: "character_id", type: "integer", nullable: false),
                lastfreerespecializeat = table.Column<DateTime>(name: "last_free_respecialize_at", type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_character_limitations", x => x.characterid);
                table.ForeignKey(
                    name: "fk_character_limitations_characters_character_id",
                    column: x => x.characterid,
                    principalTable: "characters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "character_limitations");
    }
}
