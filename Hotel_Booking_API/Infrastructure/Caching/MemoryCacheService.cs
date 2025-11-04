using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Hotel_Booking_API.Infrastructure.Caching
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _settings;
        private static readonly ConcurrentDictionary<string, byte> KeyIndex = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> PrefixToKeys = new();

        public MemoryCacheService(IMemoryCache cache, IOptions<CacheSettings> settings)
        {
            _cache = cache;
            _settings = settings.Value;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            _cache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, CacheEntrySettings? settings = null, CancellationToken cancellationToken = default)
        {
            var opts = BuildOptions(settings);
            _cache.Set(key, value, opts);
            IndexKey(key, settings?.Prefix);
            return Task.CompletedTask;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CacheEntrySettings? settings = null, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(key, out T? cached) && cached is not null)
            {
                return cached;
            }

            var created = await factory(cancellationToken);
            var opts = BuildOptions(settings);
            _cache.Set(key, created, opts);
            IndexKey(key, settings?.Prefix);
            return created;
        }

        public Task RemoveByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            _cache.Remove(key);
            KeyIndex.TryRemove(key, out _);
            foreach (var map in PrefixToKeys.Values)
            {
                map.TryRemove(key, out _);
            }
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            if (PrefixToKeys.TryGetValue(prefix, out var keys))
            {
                foreach (var k in keys.Keys)
                {
                    _cache.Remove(k);
                    KeyIndex.TryRemove(k, out _);
                }
                PrefixToKeys.TryRemove(prefix, out _);
            }
            return Task.CompletedTask;
        }

        private MemoryCacheEntryOptions BuildOptions(CacheEntrySettings? settings)
        {
            var options = new MemoryCacheEntryOptions
            {
                Priority = settings?.Priority ?? CacheItemPriority.Normal
            };

            var ttl = settings?.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromSeconds(_settings.DefaultTtlSeconds);
            options.SetAbsoluteExpiration(ttl);

            if (settings?.SlidingExpiration is not null)
            {
                options.SetSlidingExpiration(settings.SlidingExpiration.Value);
            }

            options.SetSize(settings?.Size ?? 1);
            return options;
        }

        private static void IndexKey(string key, string? prefix)
        {
            KeyIndex[key] = 1;
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var map = PrefixToKeys.GetOrAdd(prefix, _ => new ConcurrentDictionary<string, byte>());
                map[key] = 1;
            }
        }
    }
}


