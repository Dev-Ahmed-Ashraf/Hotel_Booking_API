using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hotel_Booking_API.Domain.Entities;

namespace Hotel_Booking_API.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Rating)
                .IsRequired();
                
            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            // Foreign key relationships
            builder.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Hotel)
                .WithMany(h => h.Reviews)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure rating is between 1 and 5
            builder.HasCheckConstraint("CK_Review_Rating", "Rating >= 1 AND Rating <= 5");

            // Configure soft delete
            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}
