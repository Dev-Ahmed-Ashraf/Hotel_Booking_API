using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking.IntegrationTests.Helpers
{
    public static class StripeTestHelper
    {
        public static string GenerateStripeSignature(string payload, string secret)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signedPayload = $"{timestamp}.{payload}";

            // Compute HMAC SHA256 signature
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return $"t={timestamp},v1={signature}";
        }
    }
}
