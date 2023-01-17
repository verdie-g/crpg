using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class RenameActivityLogTypes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'user_creation' TO 'user_created';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'user_deletion' TO 'user_deleted';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'user_server_join' TO 'server_joined';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'character_creation' TO 'character_created';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'character_deletion' TO 'character_deleted';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'character_respecialization' TO 'character_respecialized';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'character_retirement' TO 'character_retired';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'chat_message' TO 'chat_message_sent';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'user_created' TO 'user_creation';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'user_deleted' TO 'user_deletion';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'server_joined' TO 'user_server_join';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'character_created' TO 'character_creation';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'character_deleted' TO 'character_deletion';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'character_respecialized' TO 'character_respecialization';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'character_retired' TO 'character_retirement';");
        migrationBuilder.Sql("ALTER TYPE activity_log_type RENAME VALUE 'chat_message_sent' TO 'chat_message';");
    }
}
