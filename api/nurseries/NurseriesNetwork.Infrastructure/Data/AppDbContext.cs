using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.Entities;

namespace NurseriesNetwork.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Nursery> Nurseries { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<NurseryImage> NurseryImages { get; set; }
        public DbSet<Review> Reviews { get; set; }      
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
