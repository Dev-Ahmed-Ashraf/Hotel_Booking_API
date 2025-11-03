namespace Hotel_Booking_API.Infrastructure.Services
{
    public class StripeOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string Currency { get; set; } = "usd";
    }
}


