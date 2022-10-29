using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

public partial class AddIsDonorToUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_donor",
            table: "users",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_donor",
            table: "users");
    }
}
