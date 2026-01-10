using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.Migrations;

/// <inheritdoc />
public partial class UpdatedModel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Ongoing",
            table: "Series",
            newName: "IsPublishedOnSite");

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Volumes",
            type: "longtext",
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<bool>(
            name: "IsPublishedOnSite",
            table: "Volumes",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "Type",
            table: "Volumes",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "Status",
            table: "Series",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "Type",
            table: "Series",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Description",
            table: "Volumes");

        migrationBuilder.DropColumn(
            name: "IsPublishedOnSite",
            table: "Volumes");

        migrationBuilder.DropColumn(
            name: "Type",
            table: "Volumes");

        migrationBuilder.DropColumn(
            name: "Status",
            table: "Series");

        migrationBuilder.DropColumn(
            name: "Type",
            table: "Series");

        migrationBuilder.RenameColumn(
            name: "IsPublishedOnSite",
            table: "Series",
            newName: "Ongoing");
    }
}
