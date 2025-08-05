using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grind.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredClassesToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "ClientPreferredClasses",
                newName: "ClassCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClassCategory",
                table: "ClientPreferredClasses",
                newName: "Category");
        }
    }
}
