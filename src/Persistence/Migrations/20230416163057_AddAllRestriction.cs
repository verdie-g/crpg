using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddAllRestriction : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:restriction_type", "all,join,chat")
            .OldAnnotation("Npgsql:Enum:restriction_type", "join,chat");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:restriction_type", "join,chat")
            .OldAnnotation("Npgsql:Enum:restriction_type", "all,join,chat");
    }
}
