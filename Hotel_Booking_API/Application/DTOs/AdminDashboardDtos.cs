namespace Hotel_Booking_API.Application.DTOs
{
    public class DashboardStatsDto
    {
        public UserStatsDto Users { get; set; } = new();
        public HotelStatsDto Hotels { get; set; } = new();
        public RoomStatsDto Rooms { get; set; } = new();
        public BookingStatsDto Bookings { get; set; } = new();
        public PaymentStatsDto Payments { get; set; } = new();
        public ReviewStatsDto Reviews { get; set; } = new();
    }

    public class UserStatsDto
    {
        public int Total { get; set; }
        public int NewLast30Days { get; set; }
        public double GrowthRate { get; set; }
    }

    public class HotelStatsDto
    {
        public int Total { get; set; }
        public int NewLast30Days { get; set; }
        public double AverageRating { get; set; }
        public List<string> TopHotels { get; set; } = new();
    }

    public class RoomStatsDto
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public int Booked { get; set; }
        public double OccupancyRate { get; set; }
    }

    public class BookingStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Cancelled { get; set; }
        public int Last7Days { get; set; }
        public double CancellationRate { get; set; }
        public double AverageStayDuration { get; set; }
    }

    public class PaymentStatsDto
    {
        public double TotalRevenue { get; set; }
        public double MonthlyRevenue { get; set; }
        public double SuccessRate { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
    }

    public class ReviewStatsDto
    {
        public int Total { get; set; }
        public double AverageScore { get; set; }
        public int NewLast30Days { get; set; }
    }
}
