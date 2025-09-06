using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaShelf.DAL.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Authors",
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
                table.PrimaryKey("PK_Authors", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Countries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Name = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CountryCode = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                FlagUrl = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Countries", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "FailedSyncRecords",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Url = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ExceptionMessage = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                StackTrace = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                VolumeJson = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FailedSyncRecords", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                IdentityUserId = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                VisibleUsername = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Publishers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Name = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Url = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CountryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Publishers", x => x.Id);
                table.ForeignKey(
                    name: "FK_Publishers_Countries_CountryId",
                    column: x => x.CountryId,
                    principalTable: "Countries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Series",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Title = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                OriginalName = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Aliases = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                MalId = table.Column<int>(type: "int", nullable: false),
                Ongoing = table.Column<bool>(type: "tinyint(1)", nullable: false),
                TotalVolumes = table.Column<int>(type: "int", nullable: true),
                PublisherId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Series", x => x.Id);
                table.ForeignKey(
                    name: "FK_Series_Publishers_PublisherId",
                    column: x => x.PublisherId,
                    principalTable: "Publishers",
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
                    name: "FK_AuthorSeries_Authors_AuthorsId",
                    column: x => x.AuthorsId,
                    principalTable: "Authors",
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
            name: "Volumes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Title = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Number = table.Column<int>(type: "int", nullable: false),
                ISBN = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                OneShot = table.Column<bool>(type: "tinyint(1)", nullable: false),
                SingleIssue = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CoverImageUrl = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PurchaseUrl = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IsPreorder = table.Column<bool>(type: "tinyint(1)", nullable: false),
                PreorderStart = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                ReleaseDate = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                AvgRating = table.Column<double>(type: "double", nullable: false),
                SeriesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Volumes", x => x.Id);
                table.ForeignKey(
                    name: "FK_Volumes_Series_SeriesId",
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
                    name: "FK_AuthorVolume_Authors_OverrideAuthorsId",
                    column: x => x.OverrideAuthorsId,
                    principalTable: "Authors",
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

        migrationBuilder.CreateTable(
            name: "Ownership",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                VolumeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Status = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Ownership", x => x.Id);
                table.ForeignKey(
                    name: "FK_Ownership_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Ownership_Volumes_VolumeId",
                    column: x => x.VolumeId,
                    principalTable: "Volumes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
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
                Rating = table.Column<int>(type: "int", nullable: true),
                Review = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
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

        migrationBuilder.CreateIndex(
            name: "IX_AuthorSeries_SeriesId",
            table: "AuthorSeries",
            column: "SeriesId");

        migrationBuilder.CreateIndex(
            name: "IX_AuthorVolume_VolumesId",
            table: "AuthorVolume",
            column: "VolumesId");

        migrationBuilder.CreateIndex(
            name: "IX_Ownership_UserId",
            table: "Ownership",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Ownership_VolumeId",
            table: "Ownership",
            column: "VolumeId");

        migrationBuilder.CreateIndex(
            name: "IX_Publishers_CountryId",
            table: "Publishers",
            column: "CountryId");

        migrationBuilder.CreateIndex(
            name: "IX_Reading_UserId",
            table: "Reading",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Reading_VolumeId",
            table: "Reading",
            column: "VolumeId");

        migrationBuilder.CreateIndex(
            name: "IX_Series_PublisherId",
            table: "Series",
            column: "PublisherId");

        migrationBuilder.CreateIndex(
            name: "IX_Volumes_SeriesId",
            table: "Volumes",
            column: "SeriesId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AuthorSeries");

        migrationBuilder.DropTable(
            name: "AuthorVolume");

        migrationBuilder.DropTable(
            name: "FailedSyncRecords");

        migrationBuilder.DropTable(
            name: "Ownership");

        migrationBuilder.DropTable(
            name: "Reading");

        migrationBuilder.DropTable(
            name: "Authors");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "Volumes");

        migrationBuilder.DropTable(
            name: "Series");

        migrationBuilder.DropTable(
            name: "Publishers");

        migrationBuilder.DropTable(
            name: "Countries");
    }
}
