using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace MangaShelf.DAL;

public class MangaDbContext : DbContext
{
    private static readonly ValueConverter<DateTimeOffset, DateTime> DateTimeOffsetUtcConverter = new(
        value => value.UtcDateTime,
        value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc), TimeSpan.Zero));

    private static readonly ValueConverter<DateTimeOffset?, DateTime?> NullableDateTimeOffsetUtcConverter = new(
        value => value.HasValue ? value.Value.UtcDateTime : null,
        value => value.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc), TimeSpan.Zero) : null);

    // Domain tables
    public DbSet<Country> Countries { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Series> Series { get; set; }
    public DbSet<Volume> Volumes { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Ownership> Ownerships { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<Author> Authors { get; set; }

    public MangaDbContext(DbContextOptions<MangaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCollation("utf8mb4_unicode_ci");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Check if the entity type derives from BaseEntity
            if (typeof(IEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IEntity.Id))
                    .ValueGeneratedOnAdd();
            }

            // Apply a global query filter to exclude soft-deleted entities
            if (typeof(IDeletableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertToDeleteFilter(entityType.ClrType));
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion(DateTimeOffsetUtcConverter);
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion(NullableDateTimeOffsetUtcConverter);
                }
            }
        }

        modelBuilder.Entity<Series>()
            .Property(e => e.Aliases)
            .HasConversion(
                v => string.Join('|', v),
                v => v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList());

        modelBuilder.Entity<Reading>().ToTable("Reading");
        modelBuilder.Entity<Ownership>().ToTable("Ownership");



        modelBuilder.Entity<Volume>()
            .HasOne(v => v.Series)
            .WithMany(s => s.Volumes)
            .HasForeignKey(v => v.SeriesId);

        modelBuilder.Entity<Volume>()
            .HasIndex(v => new { v.SeriesId, v.Number, v.Title }).IsUnique();

        modelBuilder.Entity<Likes>()
            .HasIndex(l => new { l.UserId, l.VolumeId }).IsUnique();

        modelBuilder.Entity<Country>()
            .HasIndex(c => c.Name);

        modelBuilder.Entity<Author>()
            .HasIndex(a => a.Name);
    }

    private static LambdaExpression ConvertToDeleteFilter(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Property(parameter, nameof(IDeletableEntity.IsDeleted));
        var constant = Expression.Constant(false);
        var body = Expression.Equal(property, constant);
        return Expression.Lambda(body, parameter);
    }
}
