using Hotel_Booking_API.Application.Features.Payments.Services;
using Hotel_Booking_API.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Stripe;

namespace Hotel_Booking_API.Controllers
{
    /// <summary>
    /// Controller for handling Stripe webhook events.
    /// Processes payment-related events from Stripe and updates payment status accordingly.
    /// </summary>
    [ApiController]
    [Route("api/stripe/webhook")]
    public class StripeWebhooksController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly IPaymentUpdateService _paymentUpdateService;

        public StripeWebhooksController(
            IStripeService stripeService,
            IPaymentUpdateService paymentUpdateService)
        {
            _stripeService = stripeService;
            _paymentUpdateService = paymentUpdateService;
        }

        
        /// <summary>
        /// Handles incoming Stripe webhook events.
        /// Validates the webhook signature and processes payment-related events.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>200 OK if event is processed successfully, 400/401 for invalid requests</returns>
        [HttpPost]
        public async Task<IActionResult> Handle(CancellationToken cancellationToken)
        {
            // Read request body
            string json;
            using (var reader = new StreamReader(Request.Body))
            {
                json = await reader.ReadToEndAsync(cancellationToken);
            }

            // Get signature header
            var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                Log.Warning("Stripe webhook received without signature header");
                return BadRequest(new { error = "Missing Stripe-Signature header" });
            }

            // Verify webhook signature
            Event stripeEvent;
            try
            {
                stripeEvent = _stripeService.VerifyWebhookSignature(json, signatureHeader);
            }
            catch (StripeException ex)
            {
                Log.Warning(ex, "Stripe webhook signature verification failed");
                return Unauthorized(new { error = "Invalid webhook signature" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error during webhook signature verification");
                return Unauthorized(new { error = "Webhook verification failed" });
            }

            Log.Information(
                "Stripe webhook received: Type={EventType}, Id={EventId}, Livemode={Livemode}",
                stripeEvent.Type, stripeEvent.Id, stripeEvent.Livemode);

            // Process event asynchronously to return 200 OK quickly
            // Stripe expects a quick response (within 5 seconds)
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessWebhookEventAsync(stripeEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Error(ex,
                        "Error processing Stripe webhook event: Type={EventType}, Id={EventId}",
                        stripeEvent.Type, stripeEvent.Id);
                }
            }, cancellationToken);

            // Return 200 OK immediately to acknowledge receipt
            return Ok(new { received = true });
        }

        #region Private Methods
        /// <summary>
        /// Processes a Stripe webhook event based on its type.
        /// </summary>
        private async Task ProcessWebhookEventAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            try
            {
                switch (stripeEvent.Type)
                {
                    case Events.PaymentIntentSucceeded:
                        await HandlePaymentIntentSucceededAsync(stripeEvent, cancellationToken);
                        break;

                    case Events.PaymentIntentPaymentFailed:
                        await HandlePaymentIntentFailedAsync(stripeEvent, cancellationToken);
                        break;

                    case Events.PaymentIntentCanceled:
                        await HandlePaymentIntentCanceledAsync(stripeEvent, cancellationToken);
                        break;

                    case Events.ChargeRefunded:
                        await HandleChargeRefundedAsync(stripeEvent, cancellationToken);
                        break;

                    default:
                        Log.Information(
                            "Unhandled Stripe webhook event type: {EventType}, Id={EventId}",
                            stripeEvent.Type, stripeEvent.Id);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Error processing Stripe webhook event: Type={EventType}, Id={EventId}",
                    stripeEvent.Type, stripeEvent.Id);
                throw;
            }
        }

        #region Event Handlers
        private async Task HandlePaymentIntentSucceededAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                Log.Warning("PaymentIntent is null in payment_intent.succeeded event: EventId={EventId}", stripeEvent.Id);
                return;
            }

            await _paymentUpdateService.HandlePaymentIntentSucceededAsync(
                paymentIntent,
                stripeEvent.Id,
                cancellationToken);
        }
        private async Task HandlePaymentIntentFailedAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                Log.Warning("PaymentIntent is null in payment_intent.payment_failed event: EventId={EventId}", stripeEvent.Id);
                return;
            }

            await _paymentUpdateService.HandlePaymentIntentFailedAsync(
                paymentIntent,
                stripeEvent.Id,
                cancellationToken);
        }
        private async Task HandlePaymentIntentCanceledAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                Log.Warning("PaymentIntent is null in payment_intent.canceled event: EventId={EventId}", stripeEvent.Id);
                return;
            }

            await _paymentUpdateService.HandlePaymentIntentCanceledAsync(
                paymentIntent,
                stripeEvent.Id,
                cancellationToken);
        }
        private async Task HandleChargeRefundedAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null)
            {
                Log.Warning("Charge is null in charge.refunded event: EventId={EventId}", stripeEvent.Id);
                return;
            }

            await _paymentUpdateService.HandleChargeRefundedAsync(
                charge,
                stripeEvent.Id,
                cancellationToken);
        }
        #endregion
        #endregion
    }
}


