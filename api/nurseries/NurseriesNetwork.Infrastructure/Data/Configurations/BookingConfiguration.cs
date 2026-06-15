using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Enums;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.TotalPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(b => b.Status)
            .HasConversion<string>()   // to store "Pending", "Confirmed", "Cancelled" in DB
            .HasDefaultValue(BookingStatus.Pending);

        // Relationship: Booking - Parent
        builder.HasOne(b => b.Parent)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // prevent deleting Parent while Bookings exist

        // Relationship: Booking - Nursery
        builder.HasOne(b => b.Nursery)
            .WithMany(n => n.Bookings)
            .HasForeignKey(b => b.NurseryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Booking - Child
        builder.HasOne(b => b.Child)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.ChildId)
            .OnDelete(DeleteBehavior.Restrict); // prevent deleting Child while Bookings exist
    }
}