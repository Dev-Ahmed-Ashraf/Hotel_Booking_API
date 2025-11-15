using Stripe;

namespace Hotel_Booking_API.Application.Features.Payments.Services
{
    /// <summary>
    /// Service for updating payment status based on Stripe webhook events.
    /// Handles transaction safety, idempotency, validation, and event publishing.
    /// </summary>
    public interface IPaymentUpdateService
    {
        /// <summary>
        /// Handles payment_intent.succeeded event.
        /// </summary>
        Task HandlePaymentIntentSucceededAsync(PaymentIntent paymentIntent, string stripeEventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles payment_intent.payment_failed event.
        /// </summary>
        Task HandlePaymentIntentFailedAsync(PaymentIntent paymentIntent, string stripeEventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles payment_intent.canceled event.
        /// </summary>
        Task HandlePaymentIntentCanceledAsync(PaymentIntent paymentIntent, string stripeEventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles charge.refunded event.
        /// </summary>
        Task HandleChargeRefundedAsync(Charge charge, string stripeEventId, CancellationToken cancellationToken = default);
    }
}

