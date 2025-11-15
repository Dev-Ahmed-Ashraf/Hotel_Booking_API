using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Infrastructure.Caching;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace Hotel_Booking_API.Application.Features.Hotels.Queries.GetHotels
{
    public class GetHotelsQuery : IRequest<ApiResponse<PagedList<HotelDto>>>, ICacheKeyProvider
    {
        public PaginationParameters Pagination { get; set; } = new();
        public SearchHotelsDto? Search { get; set; }
        public bool IncludeDeleted { get; set; }

        public string GetCacheKey()
        {
            var payload = $"p={Pagination.PageNumber}:{Pagination.PageSize}|city={Search?.City}|country={Search?.Country}|min={Search?.MinRating}|max={Search?.MaxRating}|del={IncludeDeleted}";
            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant()[..16];
            return CacheKeys.Hotels.List(hash);
        }
        public string? GetCacheProfile() => CacheProfiles.Hotels.List;
    }
}
