using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class RenameRegions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"ALTER TYPE region RENAME VALUE 'europe' TO 'eu';");
        migrationBuilder.Sql(@"ALTER TYPE region RENAME VALUE 'north_america' TO 'na';");
        migrationBuilder.Sql(@"ALTER TYPE region RENAME VALUE 'asia' TO 'as';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"ALTER TYPE region RENAME VALUE 'eu' TO 'europe';");
        migrationBuilder.Sql(@"ALTER TYPE region RENAME VALUE 'na' TO 'north_america';");
        migrationBuilder.Sql(@"ALTER TYPE region RENAME VALUE 'as' TO 'asia';");
    }
}
