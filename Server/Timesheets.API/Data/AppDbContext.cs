using Microsoft.EntityFrameworkCore;
using Timesheets.API.Models;
using Timesheets.Shared;

namespace Timesheets.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<TimeEntryModel> TimeEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimeEntryModel>()
                .HasKey(t => t.OutlookID);
        }
    }
}
