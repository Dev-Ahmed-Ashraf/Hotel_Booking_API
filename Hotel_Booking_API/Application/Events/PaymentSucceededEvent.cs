using MediatR;

namespace Hotel_Booking_API.Application.Events
{
    public class PaymentSucceededEvent : INotification
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
}

