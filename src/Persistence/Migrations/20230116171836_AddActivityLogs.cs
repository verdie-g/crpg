using System;
using Crpg.Domain.Entities.ActivityLogs;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddActivityLogs : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_creation,user_deletion,user_server_join,user_renamed,item_bought,item_sold,character_creation,character_deletion,character_respecialization,character_retirement,chat_message,team_hit");

        migrationBuilder.CreateTable(
            name: "activity_logs",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                type = table.Column<ActivityLogType>(type: "activity_log_type", nullable: false),
                userid = table.Column<int>(name: "user_id", type: "integer", nullable: false),
                updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: false),
                createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_activity_logs", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "activity_log_metadata",
            columns: table => new
            {
                activitylogid = table.Column<int>(name: "activity_log_id", type: "integer", nullable: false),
                key = table.Column<string>(type: "text", nullable: false),
                value = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_activity_log_metadata", x => new { x.activitylogid, x.key });
                table.ForeignKey(
                    name: "fk_activity_log_metadata_activity_logs_activity_log_id",
                    column: x => x.activitylogid,
                    principalTable: "activity_logs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "activity_log_metadata");

        migrationBuilder.DropTable(
            name: "activity_logs");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_creation,user_deletion,user_server_join,user_renamed,item_bought,item_sold,character_creation,character_deletion,character_respecialization,character_retirement,chat_message,team_hit");
    }
}
