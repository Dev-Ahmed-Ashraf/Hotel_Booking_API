using MediatR;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Common;

namespace Hotel_Booking_API.Application.Features.Hotels.Commands.CreateHotel
{
    public class CreateHotelCommand : IRequest<ApiResponse<HotelDto>>
    {
        public CreateHotelDto CreateHotelDto { get; set; } = null!;
    }
}
