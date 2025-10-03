using GastroWaga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace GastroWaga.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Item> Items => Set<Item>();
        public DbSet<ItemAlias> ItemAliases => Set<ItemAlias>();
        public DbSet<CategoryDensity> CategoryDensities => Set<CategoryDensity>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Line> Lines => Set<Line>();
        public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();

        public static string GetDbPath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GastroWaga");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "gastro.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={GetDbPath()};Cache=Shared");
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Item>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired();
                e.Property(x => x.Unit).HasDefaultValue("L");
                e.HasMany(x => x.Aliases).WithOne(a => a.Item!).HasForeignKey(a => a.ItemId).OnDelete(DeleteBehavior.Cascade);
                e.Property(x => x.Mode).HasConversion<string>();
            });

            mb.Entity<ItemAlias>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.Code, x.Type }).IsUnique();
                e.Property(x => x.Type).HasDefaultValue("EAN");
            });

            mb.Entity<CategoryDensity>(e =>
            {
                e.HasKey(x => x.Category);
            });

            mb.Entity<Session>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Status).HasConversion<string>();
                e.HasIndex(x => new { x.Status, x.LastModifiedAt });
            });

            mb.Entity<Line>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.SessionId);
                e.Property(x => x.ModeUsed).HasConversion<string>();
            });

            mb.Entity<ChangeLog>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.SessionId);
            });

            // seed domyślnych gęstości (tylko przy pierwszym utworzeniu bazy)
            mb.Entity<CategoryDensity>().HasData(
                new CategoryDensity { Category = "spirits", DefaultDensityGml = 0.95 },
                new CategoryDensity { Category = "wine", DefaultDensityGml = 0.995 },
                new CategoryDensity { Category = "beer", DefaultDensityGml = 1.01 },
                new CategoryDensity { Category = "oils", DefaultDensityGml = 0.92 },
                new CategoryDensity { Category = "syrups", DefaultDensityGml = 1.30 },
                new CategoryDensity { Category = "other", DefaultDensityGml = 1.00 }
            );
        }
    }
}
