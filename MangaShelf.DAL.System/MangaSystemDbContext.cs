using MangaShelf.DAL.Interfaces;
using MangaShelf.DAL.System.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaShelf.DAL.System;

public class MangaSystemDbContext : DbContext
{
    public DbSet<Parser> Parsers { get; set; }
    public DbSet<ParserJob> Runs { get; set; }

    public DbSet<Settings> Settings { get; set; }

    public MangaSystemDbContext(DbContextOptions<MangaSystemDbContext> options) : base(options)
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
            }
        }

        modelBuilder.Entity<ParserJob>()
            .Property(e => e.VolumesAdded)
            .HasConversion(
                v => string.Join('|', v),
                v => v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList());

        modelBuilder.Entity<ParserJob>()
           .Property(e => e.VolumesUpdated)
           .HasConversion(
               v => string.Join('|', v),
               v => v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList());


        modelBuilder.Entity<Parser>()
            .HasIndex(p => p.ParserName).IsUnique();

        modelBuilder.Entity<Settings>()
            .HasIndex(s => new { s.Section, s.Key })
            .IsUnique();
    }
}
