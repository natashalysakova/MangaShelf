using MangaShelf.DAL;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.Migrations;

[DbContext(typeof(MangaDbContext))]
[Migration("20260929151500_AddVolumeSubmissionCovers")]
public partial class AddVolumeSubmissionCovers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CoverImageUrl",
            table: "VolumeSubmissions",
            type: "longtext",
            nullable: true,
            collation: "utf8mb4_unicode_ci");

        migrationBuilder.AddColumn<string>(
            name: "CoverImageUrlSmall",
            table: "VolumeSubmissions",
            type: "longtext",
            nullable: true,
            collation: "utf8mb4_unicode_ci");

        migrationBuilder.AddColumn<string>(
            name: "OriginalCoverUrl",
            table: "VolumeSubmissions",
            type: "longtext",
            nullable: true,
            collation: "utf8mb4_unicode_ci");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "CoverImageUrl", table: "VolumeSubmissions");
        migrationBuilder.DropColumn(name: "CoverImageUrlSmall", table: "VolumeSubmissions");
        migrationBuilder.DropColumn(name: "OriginalCoverUrl", table: "VolumeSubmissions");
    }
}
