using Microsoft.Extensions.Options;
using Serilog;
using Stripe;

namespace Hotel_Booking_API.Infrastructure.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeOptions _options;
        private readonly PaymentIntentService _paymentIntentService;
        private readonly ChargeService _chargeService;

        public StripeService(IOptions<StripeOptions> options)
        {
            _options = options.Value;
            StripeConfiguration.ApiKey = _options.ApiKey;
            _paymentIntentService = new PaymentIntentService();
            _chargeService = new ChargeService();
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

        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
        {
            Log.Debug("Retrieving Stripe PaymentIntent: {PaymentIntentId}", paymentIntentId);
            var intent = await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
            Log.Debug("Retrieved Stripe PaymentIntent: {PaymentIntentId}, Status={Status}", intent.Id, intent.Status);
            return intent;
        }

        public async Task<Charge> GetChargeAsync(string chargeId, CancellationToken cancellationToken = default)
        {
            Log.Debug("Retrieving Stripe Charge: {ChargeId}", chargeId);
            var charge = await _chargeService.GetAsync(chargeId, cancellationToken: cancellationToken);
            Log.Debug("Retrieved Stripe Charge: {ChargeId}, Status={Status}", charge.Id, charge.Status);
            return charge;
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


