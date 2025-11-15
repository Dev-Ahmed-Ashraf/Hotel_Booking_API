namespace Hotel_Booking_API.Application.DTOs
{
    public class HotelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public List<RoomDto> Rooms { get; set; } = new();
    }

    public class CreateHotelDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal Rating { get; set; }
    }

    public class UpdateHotelDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public decimal? Rating { get; set; }
    }

    public class SearchHotelsDto
    {
        public string? City { get; set; }
        public string? Country { get; set; }
        public decimal? MinRating { get; set; }
        public decimal? MaxRating { get; set; }
    }
}
