using Hotel_Booking_API.Domain.Enums;

namespace Hotel_Booking_API.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for room information.
    /// Contains all room details including hotel information.
    /// </summary>
    public class RoomDto
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType Type { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for creating a new room.
    /// Contains all required fields for room creation.
    /// </summary>
    public class CreateRoomDto
    {
        public int HotelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType Type { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for updating an existing room.
    /// All fields are optional to support partial updates.
    /// </summary>
    public class UpdateRoomDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType? Type { get; set; }
        public decimal Price { get; set; }
        public bool? IsAvailable { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for searching available rooms.
    /// Used for filtering rooms based on availability criteria.
    /// </summary>
    public class AvailableRoomsDto
    {
        public int HotelId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int? Capacity { get; set; }
        public RoomType? Type { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for searching rooms with various filters.
    /// Supports filtering by hotel, type, availability, price range, and capacity.
    /// </summary>
    public class SearchRoomsDto
    {
        /// <summary>
        /// Filter by specific hotel ID.
        /// </summary>
        public int? HotelId { get; set; }
        
        /// <summary>
        /// Filter by hotel name (partial match).
        /// </summary>
        public string? HotelName { get; set; }
        
        /// <summary>
        /// Filter by room number (partial match).
        /// </summary>
        public string? RoomNumber { get; set; }
        
        /// <summary>
        /// Filter by room type.
        /// </summary>
        public RoomType? Type { get; set; }
        
        /// <summary>
        /// Filter by availability status.
        /// </summary>
        public bool? IsAvailable { get; set; }
        
        /// <summary>
        /// Filter by minimum price.
        /// </summary>
        public decimal? MinPrice { get; set; }
        
        /// <summary>
        /// Filter by maximum price.
        /// </summary>
        public decimal? MaxPrice { get; set; }
        
        /// <summary>
        /// Filter by minimum capacity.
        /// </summary>
        public int? Capacity { get; set; }
    }
}
