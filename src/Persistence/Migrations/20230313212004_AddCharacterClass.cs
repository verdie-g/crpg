using Crpg.Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCharacterClass : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:character_class", "infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer");

        migrationBuilder.AddColumn<CharacterClass>(
            name: "class",
            table: "characters",
            type: "character_class",
            nullable: false,
            defaultValue: CharacterClass.Infantry);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "class",
            table: "characters");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:Enum:character_class", "infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer");
    }
}
