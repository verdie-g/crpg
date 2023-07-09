using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddResetRatingRestrictionType : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:restriction_type", "all,join,chat,rating_reset")
            .OldAnnotation("Npgsql:Enum:restriction_type", "all,join,chat");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:restriction_type", "all,join,chat")
            .OldAnnotation("Npgsql:Enum:restriction_type", "all,join,chat,rating_reset");
    }
}
