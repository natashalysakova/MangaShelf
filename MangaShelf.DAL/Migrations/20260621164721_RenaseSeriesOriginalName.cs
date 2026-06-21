using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenaseSeriesOriginalName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OriginalName",
                table: "Series",
                newName: "OriginalTitle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OriginalTitle",
                table: "Series",
                newName: "OriginalName");
        }
    }
}
