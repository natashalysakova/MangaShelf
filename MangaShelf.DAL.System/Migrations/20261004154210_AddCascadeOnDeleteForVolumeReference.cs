using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.System.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeOnDeleteForVolumeReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolumeReferences_Runs_AddedParserJobId",
                table: "VolumeReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_VolumeReferences_Runs_UpdatedParserJobId",
                table: "VolumeReferences");

            migrationBuilder.AddForeignKey(
                name: "FK_VolumeReferences_Runs_AddedParserJobId",
                table: "VolumeReferences",
                column: "AddedParserJobId",
                principalTable: "Runs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VolumeReferences_Runs_UpdatedParserJobId",
                table: "VolumeReferences",
                column: "UpdatedParserJobId",
                principalTable: "Runs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolumeReferences_Runs_AddedParserJobId",
                table: "VolumeReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_VolumeReferences_Runs_UpdatedParserJobId",
                table: "VolumeReferences");

            migrationBuilder.AddForeignKey(
                name: "FK_VolumeReferences_Runs_AddedParserJobId",
                table: "VolumeReferences",
                column: "AddedParserJobId",
                principalTable: "Runs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VolumeReferences_Runs_UpdatedParserJobId",
                table: "VolumeReferences",
                column: "UpdatedParserJobId",
                principalTable: "Runs",
                principalColumn: "Id");
        }
    }
}
