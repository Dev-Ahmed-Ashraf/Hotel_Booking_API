using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Authentication.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<ApiResponse<AuthResponseDto>>
    {
        public CreateUserDto CreateUserDto { get; set; } = null!;
    }
}
