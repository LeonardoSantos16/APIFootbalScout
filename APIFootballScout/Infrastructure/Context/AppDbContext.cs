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
        public DbSet<Shortlist> Shortlist { get; set; }



        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<Player>().ToCollection("players");
            mb.Entity<Shortlist>().ToCollection("shortlist");
        }
    }
}
