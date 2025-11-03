using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Payments.Commands.CreatePaymentIntent
{
    public class CreatePaymentIntentCommand : IRequest<CreatePaymentIntentResponseDto>
    {
        public int BookingId { get; set; }
    }
}


