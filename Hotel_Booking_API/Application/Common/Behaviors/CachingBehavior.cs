using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Hotel_Booking_API.Application.Common.Behaviors
{
    /// <summary>
    /// A MediatR pipeline behavior that provides caching for request handlers.
    /// This behavior intercepts requests that implement ICacheKeyProvider and applies caching.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ICacheService _cache;
        private readonly CacheSettings _settings;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="cache">The cache service.</param>
        /// <param name="settings">The cache settings.</param>
        /// <param name="logger">The logger.</param>
        public CachingBehavior(
            ICacheService cache,
            IOptions<CacheSettings> settings,
            ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Pipeline handler. Perform additional behavior and await the <paramref name="next" /> delegate as necessary.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next handler in the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response from the handler or from the cache.</returns>
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // If the request doesn't implement ICacheKeyProvider, bypass caching
            if (request is not ICacheKeyProvider provider)
            {
                _logger.LogDebug("Request of type {RequestType} does not implement ICacheKeyProvider, bypassing cache",
                    typeof(TRequest).Name);
                return await next();
            }

            // Generate cache key and get cache profile
            var key = provider.GetCacheKey();
            var profile = provider.GetCacheProfile();

            _logger.LogDebug("Processing cacheable request of type {RequestType} with key: {CacheKey} and profile: {CacheProfile}",
                typeof(TRequest).Name, key, profile ?? "default");

            // Get cache entry settings based on the profile
            var entrySettings = GetCacheEntrySettings(profile);

            _logger.LogDebug("Cache settings for {RequestType}: TTL={Ttl} seconds, Priority={Priority}",
                typeof(TRequest).Name,
                entrySettings.AbsoluteExpirationRelativeToNow?.TotalSeconds ?? _settings.DefaultTtlSeconds,
                entrySettings.Priority);

            try
            {
                // Try to get the response from cache, or execute the handler and cache the result
                var response = await _cache.GetOrCreateAsync(
                    key,
                    async _ =>
                    {
                        _logger.LogDebug("Cache miss for key: {CacheKey}, executing handler", key);
                        return await next();
                    },
                    entrySettings,
                    cancellationToken);

                _logger.LogDebug("Successfully processed cached request of type {RequestType}", typeof(TRequest).Name);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cached request of type {RequestType}", typeof(TRequest).Name);
                throw;
            }
        }

        /// <summary>
        /// Gets the cache entry settings based on the specified profile.
        /// </summary>
        /// <param name="profile">The cache profile name.</param>
        /// <returns>The cache entry settings.</returns>
        private CacheEntrySettings GetCacheEntrySettings(string? profile)
        {
            return profile switch
            {
                // Admin profiles
                CacheProfiles.Admin.DashboardStats => CacheProfiles.Admin.BuildDashboardStats(_settings),

                // Hotel profiles
                CacheProfiles.Hotels.List => CacheProfiles.Hotels.BuildList(_settings),
                CacheProfiles.Hotels.Details => CacheProfiles.Hotels.BuildDetails(_settings),

                // Room profiles
                CacheProfiles.Rooms.List => CacheProfiles.Rooms.BuildList(_settings),
                CacheProfiles.Rooms.Details => CacheProfiles.Rooms.BuildDetails(_settings),

                // Booking profiles
                CacheProfiles.Bookings.List => CacheProfiles.Bookings.BuildList(_settings),
                CacheProfiles.Bookings.Details => CacheProfiles.Bookings.BuildDetails(_settings),

                // Default profile
                _ => new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.DefaultTtlSeconds),
                    Size = 1,
                    Priority = CacheItemPriority.Normal
                }
            };
        }
    }
}


