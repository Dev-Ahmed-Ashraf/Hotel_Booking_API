namespace Hotel_Booking_API.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Hotel Hotel { get; set; } = null!;
    }
}
