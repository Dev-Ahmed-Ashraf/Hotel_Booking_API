using Hotel_Booking_API.Domain.Enums;

namespace Hotel_Booking_API.Application.DTOs
{

    public class RoomDto
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType Type { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRoomDto
    {
        public int HotelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType Type { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;
    }


    public class UpdateRoomDto
    {
        public string? RoomNumber { get; set; }
        public RoomType? Type { get; set; }
        public decimal? Price { get; set; }
        public int? Capacity { get; set; }
        public string? Description { get; set; }
    }


    public class AvailableRoomsDto
    {
        public int? HotelId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public RoomType? Type { get; set; }
        public int? MinCapacity { get; set; }
        public decimal? MaxPrice { get; set; }
    }


    public class SearchRoomsDto
    {

        public int? HotelId { get; set; }
        public string? HotelName { get; set; }
        public string? RoomNumber { get; set; }
        public RoomType? Type { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? Capacity { get; set; }
    }
}
