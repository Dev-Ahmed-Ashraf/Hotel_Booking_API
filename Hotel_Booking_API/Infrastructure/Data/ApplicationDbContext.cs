using Microsoft.EntityFrameworkCore;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Infrastructure.Data.Configurations;

namespace Hotel_Booking_API.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new HotelConfiguration());
            modelBuilder.ApplyConfiguration(new RoomConfiguration());
            modelBuilder.ApplyConfiguration(new BookingConfiguration());
            modelBuilder.ApplyConfiguration(new ReviewConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@hotelbooking.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FirstName = "Admin",
                    LastName = "User",
                    Role = Domain.Enums.UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed sample hotels
            modelBuilder.Entity<Hotel>().HasData(
                new Hotel
                {
                    Id = 1,
                    Name = "Grand Palace Hotel",
                    Description = "Luxurious 5-star hotel in the heart of the city",
                    Address = "123 Main Street",
                    City = "New York",
                    Country = "USA",
                    Rating = 4.5m,
                    CreatedAt = DateTime.UtcNow
                },
                new Hotel
                {
                    Id = 2,
                    Name = "Ocean View Resort",
                    Description = "Beautiful beachfront resort with stunning ocean views",
                    Address = "456 Beach Road",
                    City = "Miami",
                    Country = "USA",
                    Rating = 4.2m,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed sample rooms
            modelBuilder.Entity<Room>().HasData(
                new Room
                {
                    Id = 1,
                    HotelId = 1,
                    RoomNumber = "101",
                    Type = Domain.Enums.RoomType.Standard,
                    Price = 150.00m,
                    IsAvailable = true,
                    Capacity = 2,
                    Description = "Comfortable standard room with city view",
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Id = 2,
                    HotelId = 1,
                    RoomNumber = "201",
                    Type = Domain.Enums.RoomType.Deluxe,
                    Price = 250.00m,
                    IsAvailable = true,
                    Capacity = 2,
                    Description = "Spacious deluxe room with premium amenities",
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Id = 3,
                    HotelId = 2,
                    RoomNumber = "301",
                    Type = Domain.Enums.RoomType.Suite,
                    Price = 400.00m,
                    IsAvailable = true,
                    Capacity = 4,
                    Description = "Luxurious suite with ocean view",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
