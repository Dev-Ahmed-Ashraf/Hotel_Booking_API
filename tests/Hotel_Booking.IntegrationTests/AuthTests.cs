using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Hotel_Booking_API.IntegrationTests
{
    public class AuthTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_Should_Return_201_When_Valid()
        {
            var request = new
            {
                FirstName = "Ahmed",
                LastName = "Ashraf",
                Email = $"user_{Guid.NewGuid()}@gmail.com",
                Password = "P@ssw0rd!",
                Role = 0
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }

        [Fact]
        public async Task Register_Should_Fail_When_Email_Already_Exists()
        {
            var request = new
            {
                FirstName = "Test",
                LastName = "User",
                Email = "existing@mail.com",
                Password = "P@ssw0rd!",
                Role = 0
            };

            // First registration
            await _client.PostAsJsonAsync("/api/auth/register", request);

            // Second time → should fail
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Conflict);

        }

        [Fact]
        public async Task Login_Should_Return_JWT_When_Valid()
        {
            // Create a new user first
            var email = $"login_{Guid.NewGuid()}@gmail.com";
            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 0
            });

            var login = new { Email = email, Password = "P@ssw0rd!" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", login);
            var jsonString = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(jsonString);

            var token = json.RootElement
                            .GetProperty("data")
                            .GetProperty("token")
                            .GetString();

            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_Should_Fail_When_Password_Wrong()
        {
            var email = $"wrongpass_{Guid.NewGuid()}@gmail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = "P@ssw0rd!",
                Role = 0
            });

            var login = new { Email = email, Password = "Wrong123!" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", login);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        }


        [Fact]
        public async Task Booking_Endpoint_Without_Token_Should_Return_401()
        {
            var response = await _client.GetAsync("/api/bookings");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}


