using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Hotel_Booking_API.Infrastructure.Caching
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, CacheEntrySettings? settings = null, CancellationToken cancellationToken = default);
        Task<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            CacheEntrySettings? settings = null,
            CancellationToken cancellationToken = default);
        Task RemoveByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    }

    public class CacheEntrySettings
    {
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public long Size { get; set; } = 1;
        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;
        public string? Prefix { get; set; }
    }
}


