namespace Hotel_Booking_API.Infrastructure.Caching
{
    public class CacheSettings
    {
        public long SizeLimitMB { get; set; } = 256;
        public int DefaultTtlSeconds { get; set; } = 300;

        // Per-profile TTLs
        public int AdminDashboardStatsSeconds { get; set; } = 60;
        public int EmailTemplateSeconds { get; set; } = 600;
        public int HotelsListSeconds { get; set; } = 300;
        public int HotelDetailsSeconds { get; set; } = 600;
        public int RoomsListSeconds { get; set; } = 300;
        public int RoomDetailsSeconds { get; set; } = 600;
        public int BookingsListSeconds { get; set; } = 60;
        public int BookingDetailsSeconds { get; set; } = 120;
    }
}


