using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingById
{
    /// <summary>
    /// Query for retrieving a specific booking by its ID.
    /// Returns full booking details including hotel, room, and user info.
    /// </summary>
    public class GetBookingByIdQuery : IRequest<ApiResponse<BookingDto>>, ICacheKeyProvider
    {
        public int Id { get; set; }

        public string GetCacheKey() => CacheKeys.Bookings.Details(Id);
        public string? GetCacheProfile() => CacheProfiles.Bookings.Details;
    }
}
