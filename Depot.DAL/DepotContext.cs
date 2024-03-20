using Depot.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Depot.DAL
{
    public class DepotContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        public const string UsersPath = "Users.json";
        public const string ToursPath = "Tours.json";
        public const string GroupsPath = "Groups.json";
        public const string TicketsPath = "Tickets.json";

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

        public async void LoadContext()
        {
            LoadJson(Users, UsersPath);
            LoadJson(Tours, TicketsPath);
            LoadJson(Groups, ToursPath);
            LoadJson(Tickets, GroupsPath);
            await SaveChangesAsync();
        }

        public override int SaveChanges()
        {
            int changes = base.SaveChanges();

            File.WriteAllText(UsersPath, JsonSerializer.Serialize(Users.ToList()));
            File.WriteAllText(TicketsPath, JsonSerializer.Serialize(Tickets.ToList()));
            File.WriteAllText(ToursPath, JsonSerializer.Serialize(Tours.ToList()));
            File.WriteAllText(GroupsPath, JsonSerializer.Serialize(Groups.ToList()));

            return changes;
        }

        private void LoadJson<T>(DbSet<T> dbSet, string jsonFile) where T : DbEntity
        {
            if (File.Exists(jsonFile))
            {
                var objs = JsonSerializer.Deserialize<List<T>>(File.ReadAllText(jsonFile));
                if (objs != null)
                {
                    dbSet.AddRange(objs);
                }
            }
        }
    }
}
