using APIFootballScout.Models.Domain;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace APIFootballScout.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Player> Players { get; set; }
        public DbSet<ScoutingReporter> Reports { get; set; }
        public DbSet<Shortlist> Shortlist { get; set; }



        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<Player>().ToCollection("players");
            mb.Entity<ScoutingReporter>().ToCollection("reports");
            mb.Entity<Shortlist>().ToCollection("shortlist");
        }
    }
}
