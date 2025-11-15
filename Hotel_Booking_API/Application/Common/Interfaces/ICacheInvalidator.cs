namespace Hotel_Booking_API.Application.Common.Interfaces
{
    public interface ICacheInvalidator
    {
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    }
}


