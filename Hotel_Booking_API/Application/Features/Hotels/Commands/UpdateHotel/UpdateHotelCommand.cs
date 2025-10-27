using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking.Application.Features.Hotels.Commands.UpdateHotel
{
    public class UpdateHotelCommand : IRequest<ApiResponse<HotelDto>>
    {
        public int Id { get; set; } 
        public UpdateHotelDto UpdateHotelDto { get; set; } = null!;
    }
}
