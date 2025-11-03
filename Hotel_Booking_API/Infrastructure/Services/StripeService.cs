using Microsoft.Extensions.Options;
using Serilog;
using Stripe;

namespace Hotel_Booking_API.Infrastructure.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeOptions _options;
        private readonly PaymentIntentService _paymentIntentService;

        public StripeService(IOptions<StripeOptions> options)
        {
            _options = options.Value;
            StripeConfiguration.ApiKey = _options.ApiKey;
            _paymentIntentService = new PaymentIntentService();
        }

        public async Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
            decimal amount,
            string currency,
            string description,
            string idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100m), // convert to smallest currency unit
                Currency = currency,
                Description = description,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var requestOptions = new RequestOptions
            {
                IdempotencyKey = idempotencyKey
            };

            Log.Information("Creating Stripe PaymentIntent: {Description} {Amount} {Currency} Idempotency={Idempotency}", description, amount, currency, idempotencyKey);
            var intent = await _paymentIntentService.CreateAsync(options, requestOptions, cancellationToken);
            Log.Information("Stripe PaymentIntent created: {PaymentIntentId}", intent.Id);

            return (intent.Id, intent.ClientSecret!);
        }

        public Event VerifyWebhookSignature(string json, string signatureHeader)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _options.WebhookSecret);
                return stripeEvent;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Stripe webhook signature verification failed");
                throw;
            }
        }
    }
}


