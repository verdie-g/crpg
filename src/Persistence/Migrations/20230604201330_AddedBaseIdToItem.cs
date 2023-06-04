using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddedBaseIdToItem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_user_items_items_base_item_id",
            table: "user_items");

        migrationBuilder.DropIndex(
            name: "ix_user_items_base_item_id",
            table: "user_items");

        migrationBuilder.DropIndex(
            name: "ix_user_items_user_id_base_item_id_rank",
            table: "user_items");

        migrationBuilder.AddColumn<string>(
            name: "item_id",
            table: "user_items",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "base_id",
            table: "items",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "ix_user_items_item_id",
            table: "user_items",
            column: "item_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_items_user_id_item_id_rank",
            table: "user_items",
            columns: new[] { "user_id", "item_id", "rank" },
            unique: true);

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

        migrationBuilder.DropIndex(
            name: "ix_user_items_item_id",
            table: "user_items");

        migrationBuilder.DropIndex(
            name: "ix_user_items_user_id_item_id_rank",
            table: "user_items");

        migrationBuilder.DropColumn(
            name: "item_id",
            table: "user_items");

        migrationBuilder.DropColumn(
            name: "base_id",
            table: "items");

        migrationBuilder.CreateIndex(
            name: "ix_user_items_base_item_id",
            table: "user_items",
            column: "base_item_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_items_user_id_base_item_id_rank",
            table: "user_items",
            columns: new[] { "user_id", "base_item_id", "rank" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "fk_user_items_items_base_item_id",
            table: "user_items",
            column: "base_item_id",
            principalTable: "items",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }
}
