using Hotel_Booking_API.Domain.Enums;

namespace Hotel_Booking_API.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
        public virtual Payment? Payment { get; set; }
    }
}
