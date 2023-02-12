using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class IndexActivityLog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_activity_logs_created_at_user_id",
            table: "activity_logs",
            columns: new[] { "created_at", "user_id" });

        migrationBuilder.CreateIndex(
            name: "ix_activity_logs_user_id",
            table: "activity_logs",
            column: "user_id");

        migrationBuilder.AddForeignKey(
            name: "fk_activity_logs_users_user_id",
            table: "activity_logs",
            column: "user_id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_activity_logs_users_user_id",
            table: "activity_logs");

        migrationBuilder.DropIndex(
            name: "ix_activity_logs_created_at_user_id",
            table: "activity_logs");

        migrationBuilder.DropIndex(
            name: "ix_activity_logs_user_id",
            table: "activity_logs");
    }
}
