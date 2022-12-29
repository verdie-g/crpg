using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddRegionToUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Region>(
            name: "region",
            table: "users",
            type: "region",
            nullable: true);

        migrationBuilder.Sql("UPDATE users SET region = (SELECT c.region FROM clan_members cm LEFT JOIN clans c ON cm.clan_id = c.id WHERE cm.user_id = users.id)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "region",
            table: "users");
    }
}
