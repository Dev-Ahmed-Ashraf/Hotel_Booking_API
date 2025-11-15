using Hotel_Booking_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel_Booking_API.Infrastructure.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.CheckInDate)
                .IsRequired();

            builder.Property(b => b.CheckOutDate)
                .IsRequired();

            builder.Property(b => b.TotalPrice)
                .HasPrecision(10, 2);

            builder.Property(b => b.Status)
                .IsRequired()
                .HasConversion<int>();

            // Foreign key relationships
            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure soft delete
            builder.HasQueryFilter(b => !b.IsDeleted);
        }
    }
}
