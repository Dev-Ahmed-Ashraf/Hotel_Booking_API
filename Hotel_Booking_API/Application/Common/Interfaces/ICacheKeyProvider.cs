namespace Hotel_Booking_API.Application.Common.Interfaces
{
    public interface ICacheKeyProvider
    {
        string GetCacheKey();
        string? GetCacheProfile();
    }
}


