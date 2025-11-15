using Hotel_Booking_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel_Booking_API.Infrastructure.Data.Configurations
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(h => h.Description)
                .HasMaxLength(1000);

            builder.Property(h => h.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(h => h.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(h => h.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(h => h.Rating)
                .HasPrecision(3, 2);

            // Configure soft delete
            builder.HasQueryFilter(h => !h.IsDeleted);
        }
    }
}
