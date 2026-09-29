using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddVolumeSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VolumeSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SubmittedByIdentityUserId = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SeriesId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ExistingSeriesTitle = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    ExistingPublisherName = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    NewSeriesTitle = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    NewSeriesOriginalTitle = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    NewSeriesType = table.Column<int>(type: "int", nullable: false),
                    NewSeriesStatus = table.Column<int>(type: "int", nullable: false),
                    NewSeriesTotalVolumes = table.Column<int>(type: "int", nullable: true),
                    PublisherId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NewPublisherName = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    NewPublisherUrl = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    NewPublisherCountryId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    Number = table.Column<int>(type: "int", nullable: true),
                    ISBN = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    AgeRestriction = table.Column<int>(type: "int", nullable: false),
                    PurchaseUrl = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    Description = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    IsPreorder = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PreorderStart = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SingleIssue = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ApprovedVolumeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ReviewedByIdentityUserId = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewComment = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolumeSubmissions", x => x.Id);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_VolumeSubmissions_Status",
                table: "VolumeSubmissions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolumeSubmissions");
        }
    }
}
