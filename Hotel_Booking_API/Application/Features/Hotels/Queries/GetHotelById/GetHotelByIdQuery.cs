using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using MediatR;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;

namespace Hotel_Booking.Application.Features.Hotels.Queries.GetHotelById
{
    public class GetHotelByIdQuery : IRequest<ApiResponse<HotelDto>>, ICacheKeyProvider
    {
        public int Id { get; set; } 

        public string GetCacheKey() => CacheKeys.Hotels.Details(Id);
        public string? GetCacheProfile() => CacheProfiles.Hotels.Details;
    }
}
