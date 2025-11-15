using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Hotel_Booking_API.Infrastructure.Caching
{
    /// <summary>
    /// Implementation of ICacheService using IMemoryCache with support for key prefixes and cache invalidation.
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _settings;
        private readonly ILogger<MemoryCacheService> _logger;

        // Thread-safe dictionaries for tracking keys and their prefixes
        private static readonly ConcurrentDictionary<string, byte> KeyIndex = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> PrefixToKeys = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheService"/> class.
        /// </summary>
        /// <param name="cache">The memory cache instance.</param>
        /// <param name="settings">The cache settings.</param>
        /// <param name="logger">The logger instance.</param>
        public MemoryCacheService(
            IMemoryCache cache,
            IOptions<CacheSettings> settings,
            ILogger<MemoryCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("MemoryCacheService initialized with default TTL: {DefaultTtl} seconds",
                _settings.DefaultTtlSeconds);
        }

        /// <inheritdoc/>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Attempting to get cached value for key: {CacheKey}", key);
            var found = _cache.TryGetValue(key, out T? value);

            if (found)
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", key);
            }
            else
            {
                _logger.LogDebug("Cache miss for key: {CacheKey}", key);
            }

            return Task.FromResult(value);
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key, T value, CacheEntrySettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Setting cache value for key: {CacheKey}", key);
            var opts = BuildOptions(settings);

            _cache.Set(key, value, opts);
            IndexKey(key, settings?.Prefix);

            _logger.LogDebug("Successfully set cache for key: {CacheKey}", key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            CacheEntrySettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("GetOrCreateAsync called for key: {CacheKey}", key);

            // Try to get from cache first
            if (_cache.TryGetValue(key, out T? cached) && cached is not null)
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", key);
                return cached;
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}, invoking factory method", key);

            // If not in cache, create it using the factory
            var created = await factory(cancellationToken);

            // Cache the created value
            if (created != null)
            {
                var opts = BuildOptions(settings);
                _cache.Set(key, created, opts);
                IndexKey(key, settings?.Prefix);
                _logger.LogDebug("Successfully cached value for key: {CacheKey}", key);
            }
            else
            {
                _logger.LogWarning("Factory method returned null for key: {CacheKey}", key);
            }

            return created;
        }

        /// <inheritdoc/>
        public Task RemoveByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Removing cache entry with key: {CacheKey}", key);

            // Remove from cache
            _cache.Remove(key);

            // Remove from key index
            KeyIndex.TryRemove(key, out _);

            // Remove from all prefix maps
            foreach (var map in PrefixToKeys.Values)
            {
                map.TryRemove(key, out _);
            }

            _logger.LogDebug("Successfully removed cache entry with key: {CacheKey}", key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                _logger.LogWarning("Attempted to remove cache entries with null or empty prefix");
                return Task.CompletedTask;
            }

            _logger.LogInformation("Removing all cache entries with prefix: {CachePrefix}", prefix);

            if (PrefixToKeys.TryGetValue(prefix, out var keys))
            {
                int removedCount = 0;
                foreach (var key in keys.Keys)
                {
                    _cache.Remove(key);
                    KeyIndex.TryRemove(key, out _);
                    removedCount++;
                }

                PrefixToKeys.TryRemove(prefix, out _);
                _logger.LogInformation("Removed {RemovedCount} cache entries with prefix: {CachePrefix}",
                    removedCount, prefix);
            }
            else
            {
                _logger.LogDebug("No cache entries found with prefix: {CachePrefix}", prefix);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Builds MemoryCacheEntryOptions based on the provided settings.
        /// </summary>
        /// <param name="settings">The cache entry settings.</param>
        /// <returns>Configured MemoryCacheEntryOptions.</returns>
        private MemoryCacheEntryOptions BuildOptions(CacheEntrySettings? settings)
        {
            var options = new MemoryCacheEntryOptions
            {
                Priority = settings?.Priority ?? CacheItemPriority.Normal
            };

            // Set absolute expiration (time-to-live)
            var ttl = settings?.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromSeconds(_settings.DefaultTtlSeconds);
            options.SetAbsoluteExpiration(ttl);
            _logger.LogDebug("Set absolute expiration to {Ttl} for cache entry", ttl);

            // Set sliding expiration if specified
            if (settings?.SlidingExpiration is not null)
            {
                options.SetSlidingExpiration(settings.SlidingExpiration.Value);
                _logger.LogDebug("Set sliding expiration to {SlidingTtl} for cache entry",
                    settings.SlidingExpiration.Value);
            }

            // Set size for memory management
            var size = settings?.Size ?? 1;
            options.SetSize(size);
            _logger.LogTrace("Set cache entry size to {Size}", size);

            return options;
        }

        /// <summary>
        /// Indexes a key and its prefix for efficient prefix-based operations.
        /// </summary>
        /// <param name="key">The cache key to index.</param>
        /// <param name="prefix">The optional prefix for the key.</param>
        private static void IndexKey(string key, string? prefix)
        {
            // Add to global key index
            KeyIndex[key] = 1;

            // If prefix is provided, add to prefix-based index
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var map = PrefixToKeys.GetOrAdd(prefix, _ => new ConcurrentDictionary<string, byte>());
                map[key] = 1;
            }
        }
    }
}



