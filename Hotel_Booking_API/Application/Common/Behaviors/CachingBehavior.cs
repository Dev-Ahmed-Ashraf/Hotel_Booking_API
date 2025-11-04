using System;
using System.Threading;
using System.Threading.Tasks;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;
using MediatR;
using Microsoft.Extensions.Options;

namespace Hotel_Booking_API.Application.Common.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ICacheService _cache;
        private readonly CacheSettings _settings;

        public CachingBehavior(ICacheService cache, IOptions<CacheSettings> settings)
        {
            _cache = cache;
            _settings = settings.Value;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICacheKeyProvider provider)
            {
                return await next();
            }

            var key = provider.GetCacheKey();
            var profile = provider.GetCacheProfile();

            CacheEntrySettings entrySettings = profile switch
            {
                CacheProfiles.Admin.DashboardStats => CacheProfiles.Admin.BuildDashboardStats(_settings),
                CacheProfiles.Hotels.List => CacheProfiles.Hotels.BuildList(_settings),
                CacheProfiles.Hotels.Details => CacheProfiles.Hotels.BuildDetails(_settings),
                CacheProfiles.Rooms.List => CacheProfiles.Rooms.BuildList(_settings),
                CacheProfiles.Rooms.Details => CacheProfiles.Rooms.BuildDetails(_settings),
                CacheProfiles.Bookings.List => CacheProfiles.Bookings.BuildList(_settings),
                CacheProfiles.Bookings.Details => CacheProfiles.Bookings.BuildDetails(_settings),
                _ => new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.DefaultTtlSeconds),
                    Size = 1
                }
            };

            var response = await _cache.GetOrCreateAsync(key, async _ => await next(), entrySettings, cancellationToken);
            return response;
        }
    }
}


