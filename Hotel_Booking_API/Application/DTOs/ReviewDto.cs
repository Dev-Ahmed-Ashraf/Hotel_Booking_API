using Hotel_Booking_API.Application.Common;

namespace Hotel_Booking_API.Application.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        public int HotelId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class UpdateReviewDto
    {
        public int? Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class SearchReviewsDto
    {
        public int? HotelId { get; set; }
        public int? UserId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
    }
}
