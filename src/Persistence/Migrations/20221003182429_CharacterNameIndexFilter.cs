using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations
{
    public partial class CharacterNameIndexFilter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_characters_user_id_name",
                table: "characters");

            migrationBuilder.CreateIndex(
                name: "ix_characters_user_id_name",
                table: "characters",
                columns: new[] { "user_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_characters_user_id_name",
                table: "characters");

            migrationBuilder.CreateIndex(
                name: "ix_characters_user_id_name",
                table: "characters",
                columns: new[] { "user_id", "name" },
                unique: true);
        }
    }
}
