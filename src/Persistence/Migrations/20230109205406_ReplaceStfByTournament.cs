using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class ReplaceStfByTournament : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"UPDATE characters SET
  level = 20,
  experience = 463250,
  attribute_points = 19,
  strength = 3,
  agility = 3,
  skill_points = 21,
  iron_flesh = 0,
  power_strike = 0,
  power_draw = 0,
  power_throw = 0,
  athletics = 0,
  riding = 0,
  weapon_master = 0,
  mounted_archery = 0,
  shield = 0,
  weapon_proficiency_points = 190,
  one_handed = 0,
  two_handed = 0,
  polearm = 0,
  bow = 0,
  crossbow = 0,
  throwing = 0,
  skipped_the_fun = false
WHERE skipped_the_fun = true;");
        migrationBuilder.RenameColumn(
            name: "skipped_the_fun",
            table: "characters",
            newName: "for_tournament");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "for_tournament",
            table: "characters",
            newName: "skipped_the_fun");
    }
}
