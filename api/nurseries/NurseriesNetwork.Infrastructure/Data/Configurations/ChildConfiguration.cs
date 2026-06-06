using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseriesNetwork.Core.Entities;

public class ChildConfiguration : IEntityTypeConfiguration<Child>
{
    public void Configure(EntityTypeBuilder<Child> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.SpecialNeeds)
            .IsRequired(false);

        // Relationship: Child - Parent
        builder.HasOne(c => c.Parent)
            .WithMany(u => u.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}