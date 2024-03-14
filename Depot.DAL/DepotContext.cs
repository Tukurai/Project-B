using Depot.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Depot.DAL
{
    public class DepotContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "Depot");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(b => b.Id).HasName("PrimaryKey_UserId");
            modelBuilder.Entity<Ticket>().HasKey(b => b.Id).HasName("PrimaryKey_TicketId");
            modelBuilder.Entity<Tour>().HasKey(b => b.Id).HasName("PrimaryKey_TourId");
            modelBuilder.Entity<Group>().HasKey(b => b.Id).HasName("PrimaryKey_GroupId");
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

            if (File.Exists("Tickets.json"))
            {
                var tickets = JsonSerializer.Deserialize<List<Ticket>>(File.ReadAllText("Tickets.json"));
                if (tickets != null)
                {
                    Tickets.AddRange(tickets);
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

            if (File.Exists("Groups.json"))
            {
                var groups = JsonSerializer.Deserialize<List<Group>>(File.ReadAllText("Groups.json"));
                if (groups != null)
                {
                    Groups.AddRange(groups);
                    await SaveChangesAsync();
                }
            }
        }

        public override int SaveChanges()
        {
            int changes = base.SaveChanges();

            File.WriteAllText("Users.json", JsonSerializer.Serialize(Users.ToList()));
            File.WriteAllText("Tickets.json", JsonSerializer.Serialize(Tickets.ToList()));
            File.WriteAllText("Tours.json", JsonSerializer.Serialize(Tours.ToList()));
            File.WriteAllText("Groups.json", JsonSerializer.Serialize(Groups.ToList()));

            return changes;
        }
    }
}
