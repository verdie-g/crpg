using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPeasantCharacterClass : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:character_class", "peasant,infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer")
            .OldAnnotation("Npgsql:Enum:character_class", "infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:character_class", "infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer")
            .OldAnnotation("Npgsql:Enum:character_class", "peasant,infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer");
    }
}
