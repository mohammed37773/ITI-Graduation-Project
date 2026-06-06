using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseriesNetwork.Core.Entities;

public class NurseryConfiguration : IEntityTypeConfiguration<Nursery>
{
    public void Configure(EntityTypeBuilder<Nursery> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(n => n.Description)
            .IsRequired();

        builder.Property(n => n.DailyPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(n => n.AvgRating)
            .HasDefaultValue(0.0);

        builder.Property(n => n.IsVerified)
            .HasDefaultValue(false);

        builder.Property(n => n.EmbeddingVector)
            .IsRequired(false);

        // Relationship: One-to-One with (Location)
        builder.HasOne(n => n.Location)
            .WithOne(l => l.Nursery)
            .HasForeignKey<Location>(l => l.NurseryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}