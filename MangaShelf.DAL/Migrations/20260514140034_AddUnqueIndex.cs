using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUnqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Volumes_Series_SeriesId",
                table: "Volumes");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Volumes",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.DropIndex(
                name: "IX_Volumes_SeriesId",
                table: "Volumes");

            migrationBuilder.CreateIndex(
                name: "IX_Volumes_SeriesId_Number_Title",
                table: "Volumes",
                columns: new[] { "SeriesId", "Number", "Title" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Volumes_Series_SeriesId",
                table: "Volumes",
                column: "SeriesId",
                principalTable: "Series",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Volumes_Series_SeriesId",
                table: "Volumes");

            migrationBuilder.DropIndex(
                name: "IX_Volumes_SeriesId_Number_Title",
                table: "Volumes");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Volumes",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Volumes_SeriesId",
                table: "Volumes",
                column: "SeriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Volumes_Series_SeriesId",
                table: "Volumes",
                column: "SeriesId",
                principalTable: "Series",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
