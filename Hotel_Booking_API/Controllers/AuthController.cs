using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Authentication.Commands.LoginUser;
using Hotel_Booking_API.Application.Features.Authentication.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Booking_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="createUserDto">User registration details</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] CreateUserDto createUserDto)
        {
            var command = new RegisterUserCommand { CreateUserDto = createUserDto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            var command = new LoginUserCommand { LoginDto = loginDto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
