using Microsoft.Extensions.Caching.Memory;

namespace Hotel_Booking_API.Infrastructure.Caching
{
    /// <summary>
    /// Defines the contract for a caching service that provides methods to store and retrieve cached data.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a cached item by its key.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The cached item if found; otherwise, null.</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds or updates an item in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the item to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The item to cache.</param>
        /// <param name="settings">Optional cache entry settings.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        Task SetAsync<T>(string key, T value, CacheEntrySettings? settings = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item from the cache if it exists; otherwise, creates it using the provided factory method.
        /// </summary>
        /// <typeparam name="T">The type of the item to get or create.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="factory">The factory method to create the item if it's not in the cache.</param>
        /// <param name="settings">Optional cache entry settings.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The cached or newly created item.</returns>
        Task<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            CacheEntrySettings? settings = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an item from the cache by its key.
        /// </summary>
        /// <param name="key">The cache key to remove.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        Task RemoveByKeyAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all items from the cache that have keys starting with the specified prefix.
        /// </summary>
        /// <param name="prefix">The key prefix to match.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents the settings for a cache entry.
    /// </summary>
    public class CacheEntrySettings
    {
        /// <summary>
        /// Gets or sets the absolute expiration time relative to now.
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// Gets or sets the sliding expiration time.
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// Gets or sets the size of the cache entry.
        /// </summary>
        public long Size { get; set; } = 1;

        /// <summary>
        /// Gets or sets the priority for keeping the cache entry in the cache during a memory pressure triggered cleanup.
        /// </summary>
        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

        /// <summary>
        /// Gets or sets the prefix for the cache key.
        /// </summary>
        public string? Prefix { get; set; }
    }
}


