using Hotel_Booking_API.Domain.Enums;

namespace Hotel_Booking_API.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        
        // Navigation properties
        public virtual Booking Booking { get; set; } = null!;
    }
}
