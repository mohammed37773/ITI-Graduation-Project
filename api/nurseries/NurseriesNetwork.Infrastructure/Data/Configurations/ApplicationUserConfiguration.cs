using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseriesNetwork.Core.Entities;

namespace NurseriesNetwork.Infrastructure.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LocationLat)
                .IsRequired(false);

            builder.Property(u => u.LocationLng)
                .IsRequired(false);

            builder.Property(u => u.CreatedAt)
                .IsRequired();
        }
    }
}