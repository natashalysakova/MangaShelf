using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.System.Migrations
{
    /// <inheritdoc />
    public partial class ChangeVolumeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VolumesAdded",
                table: "Runs");

            migrationBuilder.DropColumn(
                name: "VolumesUpdated",
                table: "Runs");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Parsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VolumeReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VolumeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FullName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublicId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AddedParserJobId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedParserJobId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolumeReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolumeReferences_Runs_AddedParserJobId",
                        column: x => x.AddedParserJobId,
                        principalTable: "Runs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VolumeReferences_Runs_UpdatedParserJobId",
                        column: x => x.UpdatedParserJobId,
                        principalTable: "Runs",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_VolumeReferences_AddedParserJobId",
                table: "VolumeReferences",
                column: "AddedParserJobId");

            migrationBuilder.CreateIndex(
                name: "IX_VolumeReferences_UpdatedParserJobId",
                table: "VolumeReferences",
                column: "UpdatedParserJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolumeReferences");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Parsers");

            migrationBuilder.AddColumn<string>(
                name: "VolumesAdded",
                table: "Runs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VolumesUpdated",
                table: "Runs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
