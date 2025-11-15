using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.AdminDashboard.Queries
{
    public class GetDashboardStatsQuery : IRequest<DashboardStatsDto>, ICacheKeyProvider
    {
        public string GetCacheKey() => CacheKeys.Admin.DashboardStats();
        public string? GetCacheProfile() => CacheProfiles.Admin.DashboardStats;
    }

    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {Handler} for admin dashboard stats", nameof(GetDashboardStatsQueryHandler));

            var today = DateTime.UtcNow.Date;
            var last30Days = DateTime.UtcNow.AddDays(-30);
            var last7Days = DateTime.UtcNow.AddDays(-7);
            var previous30DaysStart = DateTime.UtcNow.AddDays(-60);
            var previous30DaysEnd = DateTime.UtcNow.AddDays(-30);

            // ========== USER STATS ==========
            var totalUsers = await _unitOfWork.Users.CountAsync();
            var newUsersLast30Days = await _unitOfWork.Users.CountAsync(u => u.CreatedAt >= last30Days);
            var newUsersPrevious30Days = await _unitOfWork.Users.CountAsync(u =>
                u.CreatedAt >= previous30DaysStart && u.CreatedAt < previous30DaysEnd);

            var userGrowthRate = newUsersPrevious30Days == 0
                ? (newUsersLast30Days > 0 ? 100.0 : 0.0)
                : (newUsersLast30Days - newUsersPrevious30Days) / (double)newUsersPrevious30Days * 100.0;

            var userStats = new UserStatsDto
            {
                Total = totalUsers,
                NewLast30Days = newUsersLast30Days,
                GrowthRate = Math.Round(userGrowthRate, 2)
            };

            // ========== HOTEL STATS ==========
            var totalHotels = await _unitOfWork.Hotels.CountAsync();
            var newHotelsLast30Days = await _unitOfWork.Hotels.CountAsync(h => h.CreatedAt >= last30Days);

            // Average hotel rating from reviews
            var allReviews = await _unitOfWork.Reviews.GetAllAsync();
            var averageRating = allReviews.Any()
                ? allReviews.Average(r => r.Rating)
                : 0.0;

            // Top 3 hotels by booking count
            var topHotelsQuery = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                .Where(b => !b.IsDeleted)
                .GroupBy(b => b.Room.Hotel.Name)
                .Select(g => new { HotelName = g.Key, BookingCount = g.Count() })
                .OrderByDescending(x => x.BookingCount)
                .Take(3)
                .ToListAsync(cancellationToken);

            var topHotels = topHotelsQuery.Select(h => h.HotelName).ToList();

            var hotelStats = new HotelStatsDto
            {
                Total = totalHotels,
                NewLast30Days = newHotelsLast30Days,
                AverageRating = Math.Round(averageRating, 2),
                TopHotels = topHotels
            };

            // ========== ROOM STATS ==========
            var totalRooms = await _unitOfWork.Rooms.CountAsync();

            // Count rooms currently occupied (booked with status Confirmed or Pending where today is between CheckInDate and CheckOutDate)
            var occupiedRoomsQuery = await _context.Bookings
                .Where(b => !b.IsDeleted &&
                           (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending) &&
                           b.CheckInDate <= today &&
                           b.CheckOutDate >= today)
                .Select(b => b.RoomId)
                .Distinct()
                .CountAsync(cancellationToken);

            var bookedRooms = occupiedRoomsQuery;
            var availableRooms = totalRooms - bookedRooms;
            var occupancyRate = totalRooms == 0 ? 0.0 : bookedRooms / (double)totalRooms * 100.0;

            var roomStats = new RoomStatsDto
            {
                Total = totalRooms,
                Available = availableRooms,
                Booked = bookedRooms,
                OccupancyRate = Math.Round(occupancyRate, 2)
            };

            // ========== BOOKING STATS ==========
            var totalBookings = await _unitOfWork.Bookings.CountAsync();
            var activeBookings = await _unitOfWork.Bookings.CountAsync(
                b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed);
            var cancelledBookings = await _unitOfWork.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled);
            var bookingsLast7Days = await _unitOfWork.Bookings.CountAsync(b => b.CreatedAt >= last7Days);

            var cancellationRate = totalBookings == 0
                ? 0.0
                : cancelledBookings / (double)totalBookings * 100.0;

            // Average stay duration
            var allBookings = await _unitOfWork.Bookings.GetAllAsync();
            var averageStayDuration = allBookings.Any()
                ? allBookings.Average(b => (b.CheckOutDate - b.CheckInDate).TotalDays)
                : 0.0;

            var bookingStats = new BookingStatsDto
            {
                Total = totalBookings,
                Active = activeBookings,
                Cancelled = cancelledBookings,
                Last7Days = bookingsLast7Days,
                CancellationRate = Math.Round(cancellationRate, 2),
                AverageStayDuration = Math.Round(averageStayDuration, 2)
            };

            // ========== PAYMENT STATS ==========
            var allPayments = await _unitOfWork.Payments.GetAllAsync();
            var totalRevenue = allPayments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => (double)p.Amount);

            var monthlyRevenue = allPayments
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.CreatedAt >= last30Days)
                .Sum(p => (double)p.Amount);

            var totalPayments = allPayments.Count();
            var completedPayments = allPayments.Count(p => p.Status == PaymentStatus.Completed);
            var successRate = totalPayments == 0
                ? 0.0
                : completedPayments / (double)totalPayments * 100.0;

            var pendingPayments = allPayments.Count(p => p.Status == PaymentStatus.Pending);
            var failedPayments = allPayments.Count(p => p.Status == PaymentStatus.Failed);

            var paymentStats = new PaymentStatsDto
            {
                TotalRevenue = Math.Round(totalRevenue, 2),
                MonthlyRevenue = Math.Round(monthlyRevenue, 2),
                SuccessRate = Math.Round(successRate, 2),
                PendingPayments = pendingPayments,
                FailedPayments = failedPayments
            };

            // ========== REVIEW STATS ==========
            var totalReviews = await _unitOfWork.Reviews.CountAsync();
            var averageScore = averageRating; // Already calculated above
            var newReviewsLast30Days = await _unitOfWork.Reviews.CountAsync(r => r.CreatedAt >= last30Days);

            var reviewStats = new ReviewStatsDto
            {
                Total = totalReviews,
                AverageScore = Math.Round(averageScore, 2),
                NewLast30Days = newReviewsLast30Days
            };

            // ========== BUILD RESULT ==========
            var result = new DashboardStatsDto
            {
                Users = userStats,
                Hotels = hotelStats,
                Rooms = roomStats,
                Bookings = bookingStats,
                Payments = paymentStats,
                Reviews = reviewStats
            };

            Log.Information("Completed {Handler} successfully", nameof(GetDashboardStatsQueryHandler));

            return result;
        }
    }
}
