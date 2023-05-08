using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

    /// <inheritdoc />
    public partial class RenameLastFreeRespecAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_free_respecialize_at",
                table: "character_limitations",
                newName: "last_respecialize_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_respecialize_at",
                table: "character_limitations",
                newName: "last_free_respecialize_at");
        }
    }
