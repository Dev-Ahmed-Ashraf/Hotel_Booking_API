using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Domain.Enums;

namespace Hotel_Booking_API.Application.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public PaymentDto? Payment { get; set; }
    }

    public class CreateBookingDto
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }

    public class UpdateBookingDto
    {
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
    }

    public class SearchBookingsDto
    {
        public int? HotelId { get; set; }
        public int? UserId { get; set; }
        public BookingStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CancelBookingDto
    {
        public string? Reason { get; set; }
    }

    public class ChangeBookingStatusDto
    {
        public BookingStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class BookingPriceRequestDto
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }

    public class BookingPriceResponseDto
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public decimal RoomPrice { get; set; }
        public int Days { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class UserBookingsDto
    {
        public int UserId { get; set; }
        public PaginationParameters? Pagination { get; set; }
    }
}
