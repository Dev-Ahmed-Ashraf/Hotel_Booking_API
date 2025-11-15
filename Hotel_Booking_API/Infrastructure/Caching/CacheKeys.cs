namespace Hotel_Booking_API.Infrastructure.Caching
{
    public static class CacheKeys
    {
        public static class Admin
        {
            public const string Prefix = "admin:dashboard";
            public static string DashboardStats() => $"{Prefix}:stats";
        }

        public static class Templates
        {
            public const string Prefix = "templates";
            public static string Email(string templateName) => $"{Prefix}:email:{templateName.ToLowerInvariant()}";
        }

        public static class Hotels
        {
            public const string Prefix = "hotels";
            public static string List(string hash) => $"{Prefix}:list:{hash}";
            public static string Details(int id) => $"{Prefix}:details:{id}";
        }

        public static class Rooms
        {
            public const string Prefix = "rooms";
            public static string List(string hash) => $"{Prefix}:list:{hash}";
            public static string Details(int id) => $"{Prefix}:details:{id}";
        }

        public static class Bookings
        {
            public const string Prefix = "bookings";
            public static string List(string hash) => $"{Prefix}:list:{hash}";
            public static string Details(int id) => $"{Prefix}:details:{id}";
        }
    }
}


