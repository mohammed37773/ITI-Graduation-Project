using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.Entities;

namespace NurseriesNetwork.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Nursery> Nurseries => Set<Nursery>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<NurseryImage> NurseryImages => Set<NurseryImage>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Nursery
        builder.Entity<Nursery>(entity =>
        {
            entity.Property(n => n.Name).IsRequired().HasMaxLength(200);
            entity.Property(n => n.DailyPrice).HasColumnType("decimal(18,2)");
        });

        // Location (1-to-1 مع Nursery)
        builder.Entity<Location>(entity =>
        {
            entity.HasOne(l => l.Nursery)
                  .WithOne(n => n.Location)
                  .HasForeignKey<Location>(l => l.NurseryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Review
        builder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Nursery)
                  .WithMany(n => n.Reviews)
                  .HasForeignKey(r => r.NurseryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Parent)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(r => r.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Booking
        builder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(b => b.Nursery)
                  .WithMany(n => n.Bookings)
                  .HasForeignKey(b => b.NurseryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Parent)
                  .WithMany(p => p.Bookings)
                  .HasForeignKey(b => b.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Child)
                  .WithMany(c => c.Bookings)
                  .HasForeignKey(b => b.ChildId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // NurseryImage
        builder.Entity<NurseryImage>(entity =>
        {
            entity.HasOne(i => i.Nursery)
                  .WithMany(n => n.Images)
                  .HasForeignKey(i => i.NurseryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Payment
        builder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(p => p.Booking)
                  .WithOne(b => b.Payment)
                  .HasForeignKey<Payment>(p => p.BookingId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Parent)
                  .WithMany(u => u.Payments)
                  .HasForeignKey(p => p.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Child
        builder.Entity<Child>(entity =>
        {
            entity.HasOne(c => c.Parent)
                  .WithMany(p => p.Children)
                  .HasForeignKey(c => c.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}