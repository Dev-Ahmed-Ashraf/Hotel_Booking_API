using FluentAssertions;
using Hotel_Booking.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Hotel_Booking.IntegrationTests
{
    public class PaymentTests : IClassFixture<TestWebApplicationFactory>
    {
         private readonly HttpClient _client;

        public PaymentTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private const string StripeFakeEventJson = @"
         {
             ""id"": ""evt_test_webhook"",
             ""object"": ""event"",
             ""api_version"": ""2020-08-27"",
             ""created"": 1609459200,
             ""data"": {
                 ""object"": {
                     ""id"": ""ch_test_charge"",
                     ""object"": ""charge"",
                     ""amount"": 2000,
                     ""currency"": ""usd"",
                     ""status"": ""succeeded""
                 }
             },
             ""livemode"": false,
             ""type"": ""charge.succeeded""
         }";

        [Fact]
        public async Task StripeWebhook_Should_Return_200_When_Signature_Is_Valid()
        {
            // Arrange
            var webhookSecret = "whsec_xxx_replace_in_secrets";
            var payload = StripeFakeEventJson;

            var signature = StripeTestHelper.GenerateStripeSignature(payload, webhookSecret);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/stripe/webhook")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Stripe-Signature", signature);

            // Act
            var response = await _client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine("DEBUG: " + body);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            body.Should().Contain("received");
        }


    }
}
