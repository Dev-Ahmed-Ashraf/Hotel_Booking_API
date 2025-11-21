using FluentAssertions;
using Hotel_Booking_API;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Hotel_Booking.IntegrationTests
{
    public class BookingTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public BookingTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateBooking_Should_Return_201_When_Valid()
        {
            // ========== Register + Login user ==========
            var email = $"user_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 0 // Customer
            });

            var login = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = "P@ssw0rd!"
            });

            var loginBody = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
            var token = loginBody.RootElement.GetProperty("data").GetProperty("token").GetString();
            var userId = loginBody.RootElement.GetProperty("data").GetProperty("user").GetProperty("id").GetInt32();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);


            // ========== Create Admin to create a room ==========
            var adminEmail = $"admin_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Admin",
                LastName = "User",
                Email = adminEmail,
                Password = "P@ssw0rd!",
                Role = 1 // Admin
            });

            var adminLogin = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = adminEmail,
                Password = "P@ssw0rd!"
            });

            var adminLoginJson = JsonDocument.Parse(await adminLogin.Content.ReadAsStringAsync());
            var adminToken = adminLoginJson.RootElement.GetProperty("data").GetProperty("token").GetString();

            // Set admin token
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", adminToken);


            // ========== Create Room dynamically ==========
            var createRoomRequest = new
            {
                HotelId = 1026,  
                roomNumber = $"R{Guid.NewGuid().ToString("N")[..3]}",
                type = 0,
                Capacity = 2,
                price = 500,
                description = "Room generated dynamically from test"
            };

            var roomResponse = await _client.PostAsJsonAsync("/api/rooms", createRoomRequest);
            var roomJson = JsonDocument.Parse(await roomResponse.Content.ReadAsStringAsync());
            var dynamicRoomId = roomJson.RootElement.GetProperty("data").GetProperty("id").GetInt32();


            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);



            // ========== Create Booking ==========
            var bookingRequest = new
            {
                RoomId = dynamicRoomId,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3)
            };

            var response = await _client.PostAsJsonAsync($"/api/bookings?userId={userId}", bookingRequest);

            // Debug
            var debugBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("DEBUG BOOKING: " + debugBody);

            // ========== Assert ==========
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }


        [Fact]
        public async Task CreateBooking_Should_Fail_When_Room_Not_Found()
        {
            // Register + Login
            var email = $"user_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 0
            });

            var login = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "P@ssw0rd!" });

            var loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
            var token = loginJson.RootElement.GetProperty("data").GetProperty("token").GetString();
            var userId = loginJson.RootElement.GetProperty("data").GetProperty("user").GetProperty("id").GetInt32();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var bookingRequest = new
            {
                RoomId = 999999, // Room not found
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3)
            };

            var response = await _client.PostAsJsonAsync($"/api/bookings?userId={userId}", bookingRequest);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateBooking_Should_Fail_When_Room_Already_Booked()
        {
            // Register + Login
            var email = $"user_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 0
            });

            var login = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "P@ssw0rd!" });

            var loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
            var token = loginJson.RootElement.GetProperty("data").GetProperty("token").GetString();
            var userId = loginJson.RootElement.GetProperty("data").GetProperty("user").GetProperty("id").GetInt32();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var dates = new
            {
                CheckIn = DateTime.Today.AddDays(5),
                CheckOut = DateTime.Today.AddDays(7)
            };

            // 1st booking → OK
            await _client.PostAsJsonAsync($"/api/bookings?userId={userId}", new
            {
                RoomId = 1054,
                CheckInDate = dates.CheckIn,
                CheckOutDate = dates.CheckOut
            });

            // 2nd booking same dates → should fail
            var response = await _client.PostAsJsonAsync($"/api/bookings?userId={userId}", new
            {
                RoomId = 1054,
                CheckInDate = dates.CheckIn,
                CheckOutDate = dates.CheckOut
            });

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("DEBUG: " + body);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task GetBookings_Should_Return_200_When_Admin_Accesses_It()
        {
            // 1) Register Admin
            var adminEmail = $"admin_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Admin",
                LastName = "User",
                Email = adminEmail,
                Password = "P@ssw0rd!",
                Role = 1
            });

            var login = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = adminEmail,
                Password = "P@ssw0rd!"
            });

            var loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
            var token = loginJson.RootElement.GetProperty("data").GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/bookings");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetBookingById_Should_Return_404_When_Not_Found()
        {
            var email = $"user_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 0
            });

            var login = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = "P@ssw0rd!"
            });

            var loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
            var token = loginJson.RootElement.GetProperty("data").GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/bookings/999999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
