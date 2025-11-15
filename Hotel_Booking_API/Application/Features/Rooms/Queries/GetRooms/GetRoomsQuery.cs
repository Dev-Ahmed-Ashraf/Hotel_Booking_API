using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Infrastructure.Caching;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace Hotel_Booking_API.Application.Features.Rooms.Queries.GetRooms
{
    /// <summary>
    /// Query to retrieve a paginated list of rooms with optional filtering.
    /// Supports filtering by hotel, room type, availability, price range, and capacity.
    /// </summary>
    public class GetRoomsQuery : IRequest<ApiResponse<PagedList<RoomDto>>>, ICacheKeyProvider
    {
        public PaginationParameters Pagination { get; set; } = new();
        public SearchRoomsDto? Search { get; set; }
        public bool IncludeDeleted { get; set; } = false;

        public string GetCacheKey()
        {
            var payload = $"p={Pagination.PageNumber}:{Pagination.PageSize}|hid={Search?.HotelId}|hname={Search?.HotelName}|num={Search?.RoomNumber}|type={Search?.Type}|min={Search?.MinPrice}|max={Search?.MaxPrice}|cap={Search?.Capacity}|del={IncludeDeleted}";
            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant()[..16];
            return CacheKeys.Rooms.List(hash);
        }
        public string? GetCacheProfile() => CacheProfiles.Rooms.List;
    }
}
