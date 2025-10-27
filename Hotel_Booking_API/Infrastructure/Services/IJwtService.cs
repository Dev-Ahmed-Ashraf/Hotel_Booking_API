using Hotel_Booking_API.Domain.Entities;

namespace Hotel_Booking_API.Infrastructure.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        int GetTokenExpirationMinutes();
        string? ValidateToken(string token);
    }
}
