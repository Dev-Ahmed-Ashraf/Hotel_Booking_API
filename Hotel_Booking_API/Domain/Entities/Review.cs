namespace Hotel_Booking_API.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        // AI Analysis
        public string? Sentiment { get; set; }
        public string? AiSummary { get; set; }
        public string? Issues { get; set; }
        public string? Positives { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Hotel Hotel { get; set; } = null!;
    }
}
