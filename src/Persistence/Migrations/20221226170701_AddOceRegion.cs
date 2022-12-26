using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddOceRegion : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:region", "eu,na,as,oce")
            .OldAnnotation("Npgsql:Enum:region", "eu,na,as");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:region", "eu,na,as")
            .OldAnnotation("Npgsql:Enum:region", "eu,na,as,oce");
    }
}
