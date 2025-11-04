using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;
using System.Security.Cryptography;
using System.Text;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookings
{
    /// <summary>
    /// Query for retrieving a paginated list of bookings with optional filtering.
    /// Supports filtering by hotel, user, status, and date range.
    /// </summary>
    public class GetBookingsQuery : IRequest<ApiResponse<PagedList<BookingDto>>>, ICacheKeyProvider
    {
        public PaginationParameters Pagination { get; set; } = null!;
        public SearchBookingsDto? Search { get; set; }
        public bool IncludeDeleted { get; set; } = false;

        public string GetCacheKey()
        {
            var payload = $"p={Pagination.PageNumber}:{Pagination.PageSize}|hid={Search?.HotelId}|uid={Search?.UserId}|st={Search?.Status}|start={Search?.StartDate}|end={Search?.EndDate}|del={IncludeDeleted}";
            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant()[..16];
            return CacheKeys.Bookings.List(hash);
        }

        public string? GetCacheProfile() => CacheProfiles.Bookings.List;
    }
}
