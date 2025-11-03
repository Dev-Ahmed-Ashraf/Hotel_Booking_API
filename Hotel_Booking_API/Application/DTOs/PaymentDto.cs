using Hotel_Booking_API.Domain.Enums;

namespace Hotel_Booking_API.Application.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProcessPaymentDto
    {
        public int BookingId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }

    public class CreatePaymentIntentResponseDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
