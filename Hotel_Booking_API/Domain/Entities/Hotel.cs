namespace Hotel_Booking_API.Domain.Entities
{
    public class Hotel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal Rating { get; set; }

        // Navigation properties
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
