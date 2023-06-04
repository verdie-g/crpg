using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class RenameBaseItemToItemHotfix : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_user_items_items_base_item_id",
            table: "user_items");

        migrationBuilder.RenameColumn(
            name: "base_item_id",
            table: "user_items",
            newName: "item_id");

        migrationBuilder.RenameIndex(
            name: "ix_user_items_user_id_base_item_id_rank",
            table: "user_items",
            newName: "ix_user_items_user_id_item_id_rank");

        migrationBuilder.RenameIndex(
            name: "ix_user_items_base_item_id",
            table: "user_items",
            newName: "ix_user_items_item_id");

        migrationBuilder.AddForeignKey(
            name: "fk_user_items_items_item_id",
            table: "user_items",
            column: "item_id",
            principalTable: "items",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_user_items_items_item_id",
            table: "user_items");

        migrationBuilder.RenameColumn(
            name: "item_id",
            table: "user_items",
            newName: "base_item_id");

        migrationBuilder.RenameIndex(
            name: "ix_user_items_user_id_item_id_rank",
            table: "user_items",
            newName: "ix_user_items_user_id_base_item_id_rank");

        migrationBuilder.RenameIndex(
            name: "ix_user_items_item_id",
            table: "user_items",
            newName: "ix_user_items_base_item_id");

        migrationBuilder.AddForeignKey(
            name: "fk_user_items_items_base_item_id",
            table: "user_items",
            column: "base_item_id",
            principalTable: "items",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }
}
