using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.System.Migrations
{
    /// <inheritdoc />
    public partial class removeAuditableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Settings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Settings",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Settings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Settings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Settings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Settings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Settings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
