using Depot.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Depot.DAL
{
    public class DepotContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Tour> Tours { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=depot.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (File.Exists("Users.json"))
            {
                var users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText("Users.json"));
                modelBuilder.Entity<User>().HasData(users);
            }

            if (File.Exists("Tours.json"))
            {
                var tours = JsonSerializer.Deserialize<List<Tour>>(File.ReadAllText("Tours.json"));
                modelBuilder.Entity<Tour>().HasData(tours);
            }
        }

        public void Initialize()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public async Task SaveChangesAndWriteToJsonAsync()
        {
            await SaveChangesAsync();

            File.WriteAllText("Users.json", JsonSerializer.Serialize(Users.ToList()));
            File.WriteAllText("Tours.json", JsonSerializer.Serialize(Tours.ToList()));
        }
    }
}
