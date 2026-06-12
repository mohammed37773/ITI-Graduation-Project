using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseriesNetwork.Core.Entities;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        // Constraint: Rating (1-5)
        builder.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5"));

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        // Relationship: Review - Parent 
        builder.HasOne(r => r.Parent)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Review - Nursery
        builder.HasOne(r => r.Nursery)
            .WithMany(n => n.Reviews)
            .HasForeignKey(r => r.NurseryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}