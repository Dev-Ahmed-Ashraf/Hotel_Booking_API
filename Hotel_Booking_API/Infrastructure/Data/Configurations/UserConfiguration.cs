using Hotel_Booking_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel_Booking_API.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(u => u.Email)
                .IsUnique();

            // Configure soft delete
            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
