using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

public partial class CharacterRating : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<float>(
            name: "rating",
            table: "characters",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.AddColumn<float>(
            name: "rating_deviation",
            table: "characters",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.AddColumn<float>(
            name: "rating_volatility",
            table: "characters",
            type: "real",
            nullable: false,
            defaultValue: 0f);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "rating",
            table: "characters");

        migrationBuilder.DropColumn(
            name: "rating_deviation",
            table: "characters");

        migrationBuilder.DropColumn(
            name: "rating_volatility",
            table: "characters");
    }
}
