using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddBrokenBoolToItemAndRenamedUpgradeEndpointToRepair : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_repaired,item_upgraded,character_created,character_deleted,character_respecialized,character_retired,character_rewarded,server_joined,chat_message_sent,team_hit")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_upgraded,character_created,character_deleted,character_respecialized,character_retired,character_rewarded,server_joined,chat_message_sent,team_hit");

        migrationBuilder.AddColumn<bool>(
            name: "is_broken",
            table: "user_items",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        // Update is_broken to true where rank is -1
        migrationBuilder.Sql("UPDATE user_items SET is_broken = true WHERE rank = -1");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_broken",
            table: "user_items");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_upgraded,character_created,character_deleted,character_respecialized,character_retired,character_rewarded,server_joined,chat_message_sent,team_hit")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_repaired,item_upgraded,character_created,character_deleted,character_respecialized,character_retired,character_rewarded,server_joined,chat_message_sent,team_hit");
    }
}
