using APIFootballScout.Domain;
using APIFootballScout.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using MongoDB.EntityFrameworkCore.Extensions;

namespace APIFootballScout.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Player> Players { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>().ToCollection("players");
        }
    }
}