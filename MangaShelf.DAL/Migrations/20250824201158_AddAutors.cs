using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.MangaShelf.Migrations
{
    /// <inheritdoc />
    public partial class AddAutors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorSeries_Author_AuthorsId",
                table: "AuthorSeries");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthorVolume_Author_OverrideAuthorsId",
                table: "AuthorVolume");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Author",
                table: "Author");

            migrationBuilder.RenameTable(
                name: "Author",
                newName: "Authors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Authors",
                table: "Authors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorSeries_Authors_AuthorsId",
                table: "AuthorSeries",
                column: "AuthorsId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorVolume_Authors_OverrideAuthorsId",
                table: "AuthorVolume",
                column: "OverrideAuthorsId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorSeries_Authors_AuthorsId",
                table: "AuthorSeries");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthorVolume_Authors_OverrideAuthorsId",
                table: "AuthorVolume");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Authors",
                table: "Authors");

            migrationBuilder.RenameTable(
                name: "Authors",
                newName: "Author");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Author",
                table: "Author",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorSeries_Author_AuthorsId",
                table: "AuthorSeries",
                column: "AuthorsId",
                principalTable: "Author",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorVolume_Author_OverrideAuthorsId",
                table: "AuthorVolume",
                column: "OverrideAuthorsId",
                principalTable: "Author",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
