using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.System.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Parsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ParserName = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastRun = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ParserRun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Started = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Finished = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    VolumesFound = table.Column<int>(type: "int", nullable: false),
                    VolumesAdded = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VolumesUpdated = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParserStatusId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Progress = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParserRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParserRun_Parsers_ParserStatusId",
                        column: x => x.ParserStatusId,
                        principalTable: "Parsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ParserError",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RunTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExceptionMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StackTrace = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VolumeJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParserRunId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParserError", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParserError_ParserRun_ParserRunId",
                        column: x => x.ParserRunId,
                        principalTable: "ParserRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ParserError_ParserRunId",
                table: "ParserError",
                column: "ParserRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ParserRun_ParserStatusId",
                table: "ParserRun",
                column: "ParserStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Parsers_ParserName",
                table: "Parsers",
                column: "ParserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParserError");

            migrationBuilder.DropTable(
                name: "ParserRun");

            migrationBuilder.DropTable(
                name: "Parsers");
        }
    }
}
