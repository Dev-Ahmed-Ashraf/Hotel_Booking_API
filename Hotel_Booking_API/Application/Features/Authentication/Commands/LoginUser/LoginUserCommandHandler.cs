using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Services;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Authentication.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public LoginUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task<ApiResponse<AuthResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            // Find user by email
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == request.LoginDto.Email);
            var user = users.FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.LoginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            var userDto = _mapper.Map<UserDto>(user);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpirationMinutes())
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Login successful.");
        }
    }
}
