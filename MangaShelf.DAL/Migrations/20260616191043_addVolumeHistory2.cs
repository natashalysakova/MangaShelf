using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addVolumeHistory2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PropertyName",
                table: "VolumeHistory");

            migrationBuilder.AlterColumn<string>(
                name: "OldValue",
                table: "VolumeHistory",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "NewValue",
                table: "VolumeHistory",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AddColumn<int>(
                name: "EventType",
                table: "VolumeHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "VolumeHistory");

            migrationBuilder.UpdateData(
                table: "VolumeHistory",
                keyColumn: "OldValue",
                keyValue: null,
                column: "OldValue",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "OldValue",
                table: "VolumeHistory",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "VolumeHistory",
                keyColumn: "NewValue",
                keyValue: null,
                column: "NewValue",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "NewValue",
                table: "VolumeHistory",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AddColumn<string>(
                name: "PropertyName",
                table: "VolumeHistory",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_unicode_ci");
        }
    }
}
