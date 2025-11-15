using Stripe;

namespace Hotel_Booking_API.Infrastructure.Services
{
    public interface IStripeService
    {
        Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
            decimal amount,
            string currency,
            string description,
            string idempotencyKey,
            CancellationToken cancellationToken = default);

        Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);

        Task<Charge> GetChargeAsync(string chargeId, CancellationToken cancellationToken = default);

        Event VerifyWebhookSignature(string json, string signatureHeader);
    }
}


