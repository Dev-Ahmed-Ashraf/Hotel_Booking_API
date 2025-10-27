using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking.Application.Features.Hotels.Queries.GetHotelById
{
    public class GetHotelByIdQuery : IRequest<ApiResponse<HotelDto>>
    {
        public int Id { get; set; } 
    }
}
