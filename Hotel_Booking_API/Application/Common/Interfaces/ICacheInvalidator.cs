using System.Threading;
using System.Threading.Tasks;

namespace Hotel_Booking_API.Application.Common.Interfaces
{
    public interface ICacheInvalidator
    {
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    }
}


