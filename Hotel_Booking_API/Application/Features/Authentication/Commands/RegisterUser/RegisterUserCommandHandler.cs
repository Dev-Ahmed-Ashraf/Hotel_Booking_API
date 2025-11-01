using MediatR;
using AutoMapper;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Services;

namespace Hotel_Booking_API.Application.Features.Authentication.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public RegisterUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task<ApiResponse<AuthResponseDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Check if user already exists
            var email = request.CreateUserDto.Email?.ToString() ?? string.Empty;
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email == email);
            if (existingUsers != null && existingUsers.Any())
            {
                throw new ConflictException("User with this email already exists.");
            }

            // Create new user
            var user = new User
            {
                Email = request.CreateUserDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.CreateUserDto.Password),
                FirstName = request.CreateUserDto.FirstName,
                LastName = request.CreateUserDto.LastName,
                Role = request.CreateUserDto.Role
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            var userDto = _mapper.Map<UserDto>(user);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpirationMinutes())
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "User registered successfully.");
        }
    }
}
