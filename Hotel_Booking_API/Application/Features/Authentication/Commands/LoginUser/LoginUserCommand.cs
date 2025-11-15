using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Authentication.Commands.LoginUser
{
    public class LoginUserCommand : IRequest<ApiResponse<AuthResponseDto>>
    {
        public LoginDto LoginDto { get; set; } = null!;
    }
}
