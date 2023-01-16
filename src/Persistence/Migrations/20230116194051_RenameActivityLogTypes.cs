using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class RenameActivityLogTypes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,item_bought,item_sold,character_created,character_deleted,character_respecialized,character_retired,server_joined,chat_message_sent,team_hit")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_creation,user_deletion,user_server_join,user_renamed,item_bought,item_sold,character_creation,character_deletion,character_respecialization,character_retirement,chat_message,team_hit");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_creation,user_deletion,user_server_join,user_renamed,item_bought,item_sold,character_creation,character_deletion,character_respecialization,character_retirement,chat_message,team_hit")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,item_bought,item_sold,character_created,character_deleted,character_respecialized,character_retired,server_joined,chat_message_sent,team_hit");
    }
}
