using System;
using Microsoft.Extensions.Caching.Memory;

namespace Hotel_Booking_API.Infrastructure.Caching
{
    public static class CacheProfiles
    {
        public static class Admin
        {
            public const string DashboardStats = "Admin.DashboardStats";

            public static CacheEntrySettings BuildDashboardStats(CacheSettings settings)
            {
                return new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.AdminDashboardStatsSeconds),
                    Priority = CacheItemPriority.High,
                    Size = 1,
                    Prefix = CacheKeys.Admin.Prefix
                };
            }
        }

        public static class Templates
        {
            public const string Email = "Templates.Email";

            public static CacheEntrySettings BuildEmailTemplate(CacheSettings settings, string templateName)
            {
                return new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.EmailTemplateSeconds),
                    Priority = CacheItemPriority.Low,
                    Size = 1,
                    Prefix = CacheKeys.Templates.Prefix
                };
            }
        }

        public static class Hotels
        {
            public const string List = "Hotels.List";
            public const string Details = "Hotels.Details";

            public static CacheEntrySettings BuildList(CacheSettings settings)
            {
                return new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.HotelsListSeconds),
                    Priority = CacheItemPriority.Normal,
                    Size = 1,
                    Prefix = CacheKeys.Hotels.Prefix
                };
            }

            public static CacheEntrySettings BuildDetails(CacheSettings settings)
            {
                return new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.HotelDetailsSeconds),
                    Priority = CacheItemPriority.High,
                    Size = 1,
                    Prefix = CacheKeys.Hotels.Prefix
                };
            }
        }

        public static class Rooms
        {
            public const string List = "Rooms.List";
            public const string Details = "Rooms.Details";

            public static CacheEntrySettings BuildList(CacheSettings settings)
            {
                return new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.RoomsListSeconds),
                    Priority = CacheItemPriority.Normal,
                    Size = 1,
                    Prefix = CacheKeys.Rooms.Prefix
                };
            }

            public static CacheEntrySettings BuildDetails(CacheSettings settings)
            {
                return new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.RoomDetailsSeconds),
                    Priority = CacheItemPriority.High,
                    Size = 1,
                    Prefix = CacheKeys.Rooms.Prefix
                };
            }
        }

        public static class Bookings
        {
            public const string List = "Bookings.List";
            public const string Details = "Bookings.Details";

            public static CacheEntrySettings BuildList(CacheSettings settings)
            {
                return new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.BookingsListSeconds),
                    Priority = CacheItemPriority.Low,
                    Size = 1,
                    Prefix = CacheKeys.Bookings.Prefix
                };
            }

            public static CacheEntrySettings BuildDetails(CacheSettings settings)
            {
                return new CacheEntrySettings
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.BookingDetailsSeconds),
                    Priority = CacheItemPriority.Normal,
                    Size = 1,
                    Prefix = CacheKeys.Bookings.Prefix
                };
            }
        }
    }
}


