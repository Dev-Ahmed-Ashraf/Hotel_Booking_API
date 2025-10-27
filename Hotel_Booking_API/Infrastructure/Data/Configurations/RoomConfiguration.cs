using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hotel_Booking_API.Domain.Entities;

namespace Hotel_Booking_API.Infrastructure.Data.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.RoomNumber)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(r => r.Type)
                .IsRequired()
                .HasConversion<int>();
                
            builder.Property(r => r.Price)
                .HasPrecision(10, 2);
                
            builder.Property(r => r.Capacity)
                .IsRequired();
                
            builder.Property(r => r.Description)
                .HasMaxLength(500);
                
            builder.Property(r => r.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);

            // Foreign key relationship
            builder.HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique room number per hotel
            builder.HasIndex(r => new { r.HotelId, r.RoomNumber })
                .IsUnique();

            // Configure soft delete
            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}
