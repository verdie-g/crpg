using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddActiveCharacterToUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_characters_user_id_name",
            table: "characters");

        migrationBuilder.AddColumn<int>(
            name: "active_character_id",
            table: "users",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_active_character_id",
            table: "users",
            column: "active_character_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_characters_user_id",
            table: "characters",
            column: "user_id");

        migrationBuilder.AddForeignKey(
            name: "fk_users_characters_active_character_id1",
            table: "users",
            column: "active_character_id",
            principalTable: "characters",
            principalColumn: "id");

        migrationBuilder.Sql(@"
UPDATE users u
SET active_character_id = (SELECT id FROM characters WHERE user_id = u.id ORDER BY generation DESC, level DESC LIMIT 1);
");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_users_characters_active_character_id1",
            table: "users");

        migrationBuilder.DropIndex(
            name: "ix_users_active_character_id",
            table: "users");

        migrationBuilder.DropIndex(
            name: "ix_characters_user_id",
            table: "characters");

        migrationBuilder.DropColumn(
            name: "active_character_id",
            table: "users");

        migrationBuilder.CreateIndex(
            name: "ix_characters_user_id_name",
            table: "characters",
            columns: new[] { "user_id", "name" },
            unique: true,
            filter: "deleted_at IS NULL");
    }
}
