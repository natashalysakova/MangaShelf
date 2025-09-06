using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.System.Migrations
{
    /// <inheritdoc />
    public partial class JobStructureChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastRun",
                table: "Parsers",
                newName: "NextRun");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Runs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Runs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Runs");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Runs");

            migrationBuilder.RenameColumn(
                name: "NextRun",
                table: "Parsers",
                newName: "LastRun");
        }
    }
}
