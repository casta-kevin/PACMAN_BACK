using Microsoft.EntityFrameworkCore;
using Domain.Entity;
using Infrastructure.Persistence.Configurations;

namespace Infrastructure.Persistence
{
    public class PacmanDbContext : DbContext
    {
        public PacmanDbContext(DbContextOptions<PacmanDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new PlayerConfiguration());
            modelBuilder.ApplyConfiguration(new GameSessionConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}