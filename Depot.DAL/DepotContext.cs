using Depot.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Reflection.Emit;
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
            optionsBuilder.UseInMemoryDatabase(databaseName: "Depot");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(b => b.Id).HasName("PrimaryKey_UserId");
            modelBuilder.Entity<Tour>().HasKey(b => b.Id).HasName("PrimaryKey_TourId");
        }

        public async void LoadJson()
        {
            if (File.Exists("Users.json"))
            {
                var users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText("Users.json"));
                if (users != null)
                {
                    Users.AddRange(users);
                    await SaveChangesAsync();
                }
            }

            if (File.Exists("Tours.json"))
            {
                var tours = JsonSerializer.Deserialize<List<Tour>>(File.ReadAllText("Tours.json"));
                if (tours != null)
                {
                    Tours.AddRange(tours);
                    await SaveChangesAsync();
                }
            }
        }

        public override int SaveChanges()
        {
            int changes = base.SaveChanges();

            File.WriteAllText("Users.json", JsonSerializer.Serialize(Users.ToList()));
            File.WriteAllText("Tours.json", JsonSerializer.Serialize(Tours.ToList()));

            return changes;
        }
    }
}
