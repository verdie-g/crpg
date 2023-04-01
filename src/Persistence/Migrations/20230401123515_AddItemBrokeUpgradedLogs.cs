using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddItemBrokeUpgradedLogs : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_upgraded,character_created,character_deleted,character_respecialized,character_retired,character_rewarded,server_joined,chat_message_sent,team_hit")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,character_created,character_deleted,character_respecialized,character_retired,character_rewarded,server_joined,chat_message_sent,team_hit");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,character_created,character_deleted,character_respecialized,character_retired,character_rewarded,server_joined,chat_message_sent,team_hit")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_upgraded,character_created,character_deleted,character_respecialized,character_retired,character_rewarded,server_joined,chat_message_sent,team_hit");
    }
}
