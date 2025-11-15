using Hotel_Booking_API.Domain.Enums;

namespace Hotel_Booking_API.Domain.Entities
{
    public class Room : BaseEntity
    {
        public int HotelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType Type { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public virtual Hotel Hotel { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
