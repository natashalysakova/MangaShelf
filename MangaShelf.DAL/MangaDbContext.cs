using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MangaShelf.DAL;

public class MangaDbContext : DbContext
{
    // Domain tables
    public DbSet<Country> Countries { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Series> Series { get; set; }
    public DbSet<Volume> Volumes { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Author> Authors { get; set; }

    public MangaDbContext(DbContextOptions<MangaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Check if the entity type derives from BaseEntity
            if (typeof(IEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IEntity.Id))
                    .ValueGeneratedOnAdd();

                // Apply a global query filter to exclude soft-deleted entities
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertToDeleteFilter(entityType.ClrType));
            }
        }

        modelBuilder.Entity<Series>()
            .Property(e => e.Aliases)
            .HasConversion(
                v => string.Join('|', v),
                v => v.Split('|', StringSplitOptions.RemoveEmptyEntries));

        //modelBuilder.Entity<Volume>()
        //    .HasIndex(v=> new { v.SeriesId, v.Number, v.Title }).IsUnique();

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
