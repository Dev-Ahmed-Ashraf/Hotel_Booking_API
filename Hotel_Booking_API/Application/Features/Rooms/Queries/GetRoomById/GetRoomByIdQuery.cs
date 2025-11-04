using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;

namespace Hotel_Booking_API.Application.Features.Rooms.Queries.GetRoomById
{
    /// <summary>
    /// Query to retrieve a specific room by its unique identifier.
    /// Returns detailed room information including hotel details.
    /// </summary>
    public class GetRoomByIdQuery : IRequest<ApiResponse<RoomDto>>, ICacheKeyProvider
    {
        /// <summary>
        /// The unique identifier of the room to retrieve.
        /// </summary>
        public int Id { get; set; }

        public string GetCacheKey() => CacheKeys.Rooms.Details(Id);
        public string? GetCacheProfile() => CacheProfiles.Rooms.Details;
    }
}
