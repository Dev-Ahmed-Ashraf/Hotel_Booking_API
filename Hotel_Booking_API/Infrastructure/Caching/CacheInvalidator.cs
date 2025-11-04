using System.Threading;
using System.Threading.Tasks;
using Hotel_Booking_API.Application.Common.Interfaces;

namespace Hotel_Booking_API.Infrastructure.Caching
{
    public class CacheInvalidator : ICacheInvalidator
    {
        private readonly ICacheService _cache;

        public CacheInvalidator(ICacheService cache)
        {
            _cache = cache;
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            return _cache.RemoveByPrefixAsync(prefix, cancellationToken);
        }
    }
}


