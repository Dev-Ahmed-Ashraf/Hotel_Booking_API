using MediatR;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Common;

namespace Hotel_Booking_API.Application.Features.Hotels.Queries.GetHotels
{
    public class GetHotelsQuery : IRequest<ApiResponse<PagedList<HotelDto>>>
    {
        public PaginationParameters Pagination { get; set; } = new();
        public SearchHotelsDto? Search { get; set; }
        public bool IncludeDeleted { get; set; }
    }
}
