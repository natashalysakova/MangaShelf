using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addPiublicUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverNeedAdjustment",
                table: "Volumes");

            migrationBuilder.AddColumn<string>(
                name: "OriginalCoverUrl",
                table: "Volumes",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Volumes",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Series",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalCoverUrl",
                table: "Volumes");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Volumes");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Series");

            migrationBuilder.AddColumn<bool>(
                name: "CoverNeedAdjustment",
                table: "Volumes",
                type: "tinyint(1)",
                nullable: true);
        }
    }
}
