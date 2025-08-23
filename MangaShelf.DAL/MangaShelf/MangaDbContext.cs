using MangaShelf.DAL.Interfaces;
using MangaShelf.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace MangaShelf.DAL.MangaShelf
{
    public class MangaDbContext : DbContext
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Volume> Volumes { get; set; }

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
                        .HasQueryFilter(ConvertToDeleteFilter(entityType.ClrType));
                }
            }
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

    public class MangaDbContextFactory : IDesignTimeDbContextFactory<MangaDbContext>
    {
        public MangaDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .Build();

            var connectionString = configuration.GetConnectionString("MangaDb") ?? throw new InvalidOperationException("Connection string 'MangaDb' not found.");

            var shelfDbVersion = ServerVersion.AutoDetect(connectionString);
            var options = new DbContextOptionsBuilder<MangaDbContext>()
                .UseMySql(connectionString, shelfDbVersion);
            return new MangaDbContext(options.Options);
        }
    }
}
