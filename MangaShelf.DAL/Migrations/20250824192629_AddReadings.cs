using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.MangaShelf.Migrations
{
    /// <inheritdoc />
    public partial class AddReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VolumeNumber",
                table: "Volumes",
                newName: "Number");

            migrationBuilder.AddColumn<double>(
                name: "AvgRating",
                table: "Volumes",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ISBN",
                table: "Volumes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "OneShot",
                table: "Volumes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PreorderStart",
                table: "Volumes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SingleIssue",
                table: "Volumes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Aliases",
                table: "Series",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MalId",
                table: "Series",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Series",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Ongoing",
                table: "Series",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                table: "Series",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TotalVolumes",
                table: "Series",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Ownership",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Author",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Author", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reading",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VolumeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reading_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reading_Volumes_VolumeId",
                        column: x => x.VolumeId,
                        principalTable: "Volumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuthorSeries",
                columns: table => new
                {
                    AuthorsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SeriesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorSeries", x => new { x.AuthorsId, x.SeriesId });
                    table.ForeignKey(
                        name: "FK_AuthorSeries_Author_AuthorsId",
                        column: x => x.AuthorsId,
                        principalTable: "Author",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorSeries_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuthorVolume",
                columns: table => new
                {
                    OverrideAuthorsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VolumesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorVolume", x => new { x.OverrideAuthorsId, x.VolumesId });
                    table.ForeignKey(
                        name: "FK_AuthorVolume_Author_OverrideAuthorsId",
                        column: x => x.OverrideAuthorsId,
                        principalTable: "Author",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorVolume_Volumes_VolumesId",
                        column: x => x.VolumesId,
                        principalTable: "Volumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorSeries_SeriesId",
                table: "AuthorSeries",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorVolume_VolumesId",
                table: "AuthorVolume",
                column: "VolumesId");

            migrationBuilder.CreateIndex(
                name: "IX_Reading_UserId",
                table: "Reading",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reading_VolumeId",
                table: "Reading",
                column: "VolumeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorSeries");

            migrationBuilder.DropTable(
                name: "AuthorVolume");

            migrationBuilder.DropTable(
                name: "Reading");

            migrationBuilder.DropTable(
                name: "Author");

            migrationBuilder.DropColumn(
                name: "AvgRating",
                table: "Volumes");

            migrationBuilder.DropColumn(
                name: "ISBN",
                table: "Volumes");

            migrationBuilder.DropColumn(
                name: "OneShot",
                table: "Volumes");

            migrationBuilder.DropColumn(
                name: "PreorderStart",
                table: "Volumes");

            migrationBuilder.DropColumn(
                name: "SingleIssue",
                table: "Volumes");

            migrationBuilder.DropColumn(
                name: "Aliases",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "MalId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "Ongoing",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "TotalVolumes",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Ownership");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Volumes",
                newName: "VolumeNumber");
        }
    }
}
