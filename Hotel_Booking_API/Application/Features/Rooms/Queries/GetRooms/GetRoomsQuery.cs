using MediatR;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Common;

namespace Hotel_Booking_API.Application.Features.Rooms.Queries.GetRooms
{
    /// <summary>
    /// Query to retrieve a paginated list of rooms with optional filtering.
    /// Supports filtering by hotel, room type, availability, price range, and capacity.
    /// </summary>
    public class GetRoomsQuery : IRequest<ApiResponse<PagedList<RoomDto>>>
    {
        /// <summary>
        /// Pagination parameters for the query (page number and page size).
        /// </summary>
        public PaginationParameters Pagination { get; set; } = new();
        
        /// <summary>
        /// Search criteria for filtering rooms.
        /// </summary>
        public SearchRoomsDto? Search { get; set; }
        
        /// <summary>
        /// Whether to include soft-deleted rooms in the results.
        /// </summary>
        public bool IncludeDeleted { get; set; } = false;
    }
}
