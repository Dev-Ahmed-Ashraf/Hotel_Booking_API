using MediatR;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Common;

namespace Hotel_Booking_API.Application.Features.Authentication.Commands.LoginUser
{
    public class LoginUserCommand : IRequest<ApiResponse<AuthResponseDto>>
    {
        public LoginDto LoginDto { get; set; } = null!;
    }
}
