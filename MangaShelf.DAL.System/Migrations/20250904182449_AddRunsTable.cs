using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.System.Migrations
{
    /// <inheritdoc />
    public partial class AddRunsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParserError_ParserRun_ParserRunId",
                table: "ParserError");

            migrationBuilder.DropForeignKey(
                name: "FK_ParserRun_Parsers_ParserStatusId",
                table: "ParserRun");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParserRun",
                table: "ParserRun");

            migrationBuilder.RenameTable(
                name: "ParserRun",
                newName: "Runs");

            migrationBuilder.RenameIndex(
                name: "IX_ParserRun_ParserStatusId",
                table: "Runs",
                newName: "IX_Runs_ParserStatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Runs",
                table: "Runs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParserError_Runs_ParserRunId",
                table: "ParserError",
                column: "ParserRunId",
                principalTable: "Runs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Runs_Parsers_ParserStatusId",
                table: "Runs",
                column: "ParserStatusId",
                principalTable: "Parsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParserError_Runs_ParserRunId",
                table: "ParserError");

            migrationBuilder.DropForeignKey(
                name: "FK_Runs_Parsers_ParserStatusId",
                table: "Runs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Runs",
                table: "Runs");

            migrationBuilder.RenameTable(
                name: "Runs",
                newName: "ParserRun");

            migrationBuilder.RenameIndex(
                name: "IX_Runs_ParserStatusId",
                table: "ParserRun",
                newName: "IX_ParserRun_ParserStatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParserRun",
                table: "ParserRun",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParserError_ParserRun_ParserRunId",
                table: "ParserError",
                column: "ParserRunId",
                principalTable: "ParserRun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParserRun_Parsers_ParserStatusId",
                table: "ParserRun",
                column: "ParserStatusId",
                principalTable: "Parsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
