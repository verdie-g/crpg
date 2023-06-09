using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class MoveRankToItem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_user_items_user_id_item_id_rank",
            table: "user_items");

        migrationBuilder.DropColumn(
            name: "rank",
            table: "user_items");

        migrationBuilder.AddColumn<int>(
            name: "rank",
            table: "items",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "ix_user_items_user_id_item_id",
            table: "user_items",
            columns: new[] { "user_id", "item_id" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_user_items_user_id_item_id",
            table: "user_items");

        migrationBuilder.DropColumn(
            name: "rank",
            table: "items");

        migrationBuilder.AddColumn<int>(
            name: "rank",
            table: "user_items",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "ix_user_items_user_id_item_id_rank",
            table: "user_items",
            columns: new[] { "user_id", "item_id", "rank" },
            unique: true);
    }
}
