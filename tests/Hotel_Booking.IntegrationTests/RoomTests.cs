using FluentAssertions;
using Hotel_Booking_API;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Hotel_Booking.IntegrationTests
{
    public class RoomTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RoomTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task PrintDebug(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("DEBUG RESPONSE:");
            Console.WriteLine(body);
        }


        [Fact]
        public async Task CreateRoom_Should_Return_201_When_Admin()
        {
            // 1) Register Admin
            var email = $"admin_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Admin",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 1 // Admin
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

            // ========== Create REAL Hotel for SQLite ==========
            var createHotel = await _client.PostAsJsonAsync("/api/hotels", new
            {
                Name = "Test Hotel",
                Address = "Cairo - sdspfmskmfs",
                Description = "Auto-created for integration tests",
                City = "giza",
                Country = "Egypt",
                Rating = 3
            });

            var hotelJsonString = await createHotel.Content.ReadAsStringAsync();
            Console.WriteLine("CREATE HOTEL RESPONSE: " + hotelJsonString);

            var hotelJson = JsonDocument.Parse(hotelJsonString);
            var hotelId = hotelJson.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            // Create Room
            var createRoomRequest = new
            {
                HotelId = hotelId,
                RoomNumber = $"R{Guid.NewGuid().ToString("N").Substring(0, 3)}",
                Type = 0,
                Capacity = 2,
                Price = 500,
                Description = "Room Created From Tests When Admin"
            };

            var response = await _client.PostAsJsonAsync("/api/rooms", createRoomRequest);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateRoom_Should_Return_403_When_Customer()
        {
            // 1) Register Customer
            var email = $"cust_{Guid.NewGuid()}@gmail.com";

            var register = await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 0 // Customer
            });

            Console.WriteLine("REGISTER DEBUG: " + await register.Content.ReadAsStringAsync());

            // 2) Login
            var login = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = "P@ssw0rd!"
            });

            var loginBody = await login.Content.ReadAsStringAsync();
            Console.WriteLine("LOGIN RESPONSE => " + loginBody);

            var loginJson = JsonDocument.Parse(loginBody);
            var token = loginJson.RootElement.GetProperty("data").GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 3) Try to create room (should fail)
            var roomRequest = new
            {
                HotelId = 1026, 
                RoomNumber = $"R{Guid.NewGuid().ToString("N")[..3]}",
                Type = 0,
                Capacity = 2,
                Price = 500,
                Description = "Forbidden test"
            };

            var response = await _client.PostAsJsonAsync("/api/rooms", roomRequest);

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("DEBUG: " + body);

            // 4) Assert → Forbidden
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetRoomById_Should_Return_200_When_Room_Exists()
        {
            // Register Admin
            var email = $"admin_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Admin",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 1
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

            // ========== Create REAL Hotel for SQLite ==========
            var createHotel = await _client.PostAsJsonAsync("/api/hotels", new
            {
                Name = "Test Hotel",
                Description = "Auto-created for integration tests",
                Address = "Cairo - sdspfmskmfs",
                City = "giza",
                Vountry = "Egypt",
                Rating = 3
            });

            var hotelJsonString = await createHotel.Content.ReadAsStringAsync();
            Console.WriteLine("CREATE HOTEL RESPONSE: " + hotelJsonString);

            var hotelJson = JsonDocument.Parse(hotelJsonString);
            var hotelId = hotelJson.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            // Create room
            var roomNumber = $"R{Guid.NewGuid().ToString("N").Substring(0, 3)}";

            var createRoomRequest = new
            {
                HotelId = hotelId,
                RoomNumber = roomNumber,
                Type = 0,
                Price = 500,
                Capacity = 2,
                Description = "Test Room"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/rooms", createRoomRequest);
            var createJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
            await PrintDebug(createResponse);

            var roomId = createJson.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            // GET room by id
            var response = await _client.GetAsync($"/api/rooms/{roomId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetRoomById_Should_Return_404_When_Not_Found()
        {
            // Register Admin
            var email = $"admin_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Admin",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 1
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

            // GET room by invalid id
            var response = await _client.GetAsync("/api/rooms/999999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateRoom_Should_Return_403_When_Customer()
        {
            // 1) Register Customer
            var email = $"cust_{Guid.NewGuid()}@gmail.com";

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

            var loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
            var token = loginJson.RootElement.GetProperty("data").GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 2) Try to update room
            var updateReq = new
            {
                RoomNumber = "5457",
                Type = 1,
                Capacity = 3,
                Price = 450,
                Description = "Customer trying to update From Test"
            };

            var response = await _client.PatchAsJsonAsync("/api/rooms/1058", updateReq); 

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteRoom_Should_Return_204_When_Admin()
        {
            // 1) Register Admin
            var email = $"admin_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Admin",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 1 // Admin
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

            // ========== Create REAL Hotel for SQLite ==========
            var createHotel = await _client.PostAsJsonAsync("/api/hotels", new
            {
                Name = "Test Hotel",
                Address = "Cairo - sdspfmskmfs",
                Description = "Auto-created for integration tests",
                City = "giza",
                Country = "Egypt",
                Rating = 3
            });

            var hotelJsonString = await createHotel.Content.ReadAsStringAsync();
            Console.WriteLine("CREATE HOTEL RESPONSE: " + hotelJsonString);

            var hotelJson = JsonDocument.Parse(hotelJsonString);
            var hotelId = hotelJson.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            // 2) Create room 
            var roomReq = new
            {
                HotelId = hotelId,
                RoomNumber = $"R{Guid.NewGuid().ToString("N").Substring(0, 3)}",
                Type = 0,
                Capacity = 2,
                Price = 300,
                Description = "Room to delete"
            };

            var createRes = await _client.PostAsJsonAsync("/api/rooms", roomReq);
            var createJson = JsonDocument.Parse(await createRes.Content.ReadAsStringAsync());
            var roomId = createJson.RootElement.GetProperty("data").GetProperty("id").GetInt32();

            // 3) Delete room
            var deleteResponse = await _client.DeleteAsync($"/api/rooms/{roomId}");

            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

    }
}
