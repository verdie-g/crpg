using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class MoveExperienceMultiplierToUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "experience_multiplier",
            table: "characters");

        migrationBuilder.AddColumn<float>(
            name: "experience_multiplier",
            table: "users",
            type: "real",
            nullable: false,
            defaultValue: 1f);

        migrationBuilder.Sql("UPDATE users SET experience_multiplier = LEAST(1.48, 1 + 0.03 * (SELECT COALESCE(SUM(c.generation), 0) FROM characters c WHERE users.id = user_id))");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "experience_multiplier",
            table: "users");

        migrationBuilder.AddColumn<float>(
            name: "experience_multiplier",
            table: "characters",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.Sql("UPDATE characters SET experience_multiplier = LEAST(1.48, 1 + generation * 0.03);");
    }
}
