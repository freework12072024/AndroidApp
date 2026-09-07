
using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<User> Users => Set<User>();
        public DbSet<State> States => Set<State>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<UserDevice> UserDevices => Set<UserDevice>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

            // Seed some example states and cities
            modelBuilder.Entity<State>().HasData(
                new State { Id = 1, Name = "State A" },
                new State { Id = 2, Name = "State B" }
            );

            modelBuilder.Entity<City>().HasData(
                new City { Id = 1, Name = "City A1", StateId = 1 },
                new City { Id = 2, Name = "City A2", StateId = 1 },
                new City { Id = 3, Name = "City B1", StateId = 2 },
                new City { Id = 4, Name = "City B2", StateId = 2 }
            );
        }
    }
}
