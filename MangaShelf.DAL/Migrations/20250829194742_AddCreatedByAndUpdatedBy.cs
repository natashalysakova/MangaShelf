using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.Migrations;

/// <inheritdoc />
public partial class AddCreatedByAndUpdatedBy : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Volumes",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Volumes",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Users",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Users",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Series",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Series",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Reading",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Reading",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Publishers",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Publishers",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Ownership",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Ownership",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "FailedSyncRecords",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "FailedSyncRecords",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Countries",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Countries",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Authors",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Authors",
            type: "longtext",
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Volumes");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Volumes");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Series");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Series");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Reading");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Reading");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Publishers");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Ownership");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Ownership");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "FailedSyncRecords");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "FailedSyncRecords");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Countries");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Countries");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Authors");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Authors");
    }
}
