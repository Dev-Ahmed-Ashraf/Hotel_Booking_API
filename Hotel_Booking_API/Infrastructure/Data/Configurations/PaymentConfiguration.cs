using Hotel_Booking_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel_Booking_API.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount)
                .HasPrecision(10, 2);

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.PaymentMethod)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.TransactionId)
                .HasMaxLength(100);

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("usd");

            builder.Property(p => p.StripeEventId)
                .HasMaxLength(100);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(500);

            // Foreign key relationship
            builder.HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure soft delete
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
