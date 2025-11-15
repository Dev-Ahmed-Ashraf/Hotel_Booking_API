using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Application.Events;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;
using MediatR;
using Serilog;
using Stripe;

namespace Hotel_Booking_API.Application.Features.Payments.Services
{
    /// <summary>
    /// Service for updating payment status based on Stripe webhook events.
    /// Handles transaction safety, idempotency, validation, and event publishing.
    /// </summary>
    public class PaymentUpdateService : IPaymentUpdateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ICacheInvalidator _cacheInvalidator;

        public PaymentUpdateService(
            IUnitOfWork unitOfWork,
            IMediator mediator,
            ICacheInvalidator cacheInvalidator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task HandlePaymentIntentSucceededAsync(PaymentIntent paymentIntent, string stripeEventId, CancellationToken cancellationToken = default)
        {
            Log.Information(
                "Processing payment_intent.succeeded: PaymentIntentId={PaymentIntentId}, EventId={EventId}",
                paymentIntent.Id, stripeEventId);

            // Find payment by TransactionId (which stores the PaymentIntent Id)
            var payments = await _unitOfWork.Payments.FindAsync(p => p.TransactionId == paymentIntent.Id);
            var payment = payments.FirstOrDefault();

            if (payment == null)
            {
                Log.Warning("Payment not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
                return; // Return silently - webhook should not fail for unknown payments
            }

            // Idempotency check: if event already processed, skip
            if (payment.StripeEventId == stripeEventId)
            {
                Log.Information(
                    "Event already processed: PaymentId={PaymentId}, EventId={EventId}",
                    payment.Id, stripeEventId);
                return;
            }

            // Idempotency check: if payment is already in terminal state, skip
            if (PaymentStatusTransitionValidator.IsTerminalStatus(payment.Status))
            {
                Log.Information(
                    "Payment already in terminal state: PaymentId={PaymentId}, Status={Status}, EventId={EventId}",
                    payment.Id, payment.Status, stripeEventId);
                return;
            }

            // Validate status transition
            PaymentStatusTransitionValidator.ValidateTransition(payment.Status, PaymentStatus.Completed, payment.Id);

            // Validate amount and currency match
            PaymentValidationService.ValidatePaymentIntent(paymentIntent, payment);

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Update payment status
                var oldStatus = payment.Status;
                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                payment.StripeEventId = stripeEventId;
                await _unitOfWork.Payments.UpdateAsync(payment);

                // Update booking status if pending
                var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId, cancellationToken);
                if (booking != null && booking.Status == BookingStatus.Pending)
                {
                    booking.Status = BookingStatus.Confirmed;
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                    Log.Information(
                        "Booking status updated: BookingId={BookingId}, Status={Status}",
                        booking.Id, booking.Status);
                }

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                Log.Information(
                    "Payment status updated: PaymentId={PaymentId}, OldStatus={OldStatus}, NewStatus={NewStatus}, EventId={EventId}",
                    payment.Id, oldStatus, payment.Status, stripeEventId);

                // Publish event for email notification (after successful commit)
                try
                {
                    var paymentSucceededEvent = new PaymentSucceededEvent
                    {
                        PaymentId = payment.Id,
                        BookingId = payment.BookingId,
                        UserId = booking?.UserId ?? 0,
                        Amount = payment.Amount,
                        TransactionId = payment.TransactionId
                    };

                    // Fire-and-forget: don't await to avoid blocking webhook response
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _mediator.Publish(paymentSucceededEvent, cancellationToken);
                            Log.Information("Published PaymentSucceededEvent for PaymentId: {PaymentId}", payment.Id);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error publishing PaymentSucceededEvent for PaymentId: {PaymentId}", payment.Id);
                        }
                    }, cancellationToken);

                    Log.Information("PaymentSucceededEvent queued for PaymentId: {PaymentId}", payment.Id);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error queuing PaymentSucceededEvent for PaymentId: {PaymentId}", payment.Id);
                    // Don't fail the webhook if event publishing fails
                }

                // Invalidate admin dashboard caches after successful payment
                try
                {
                    await _cacheInvalidator.RemoveByPrefixAsync(Infrastructure.Caching.CacheKeys.Admin.Prefix, cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error invalidating cache for PaymentId: {PaymentId}", payment.Id);
                    // Don't fail the webhook if cache invalidation fails
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex,
                    "Error processing payment_intent.succeeded: PaymentIntentId={PaymentIntentId}, EventId={EventId}",
                    paymentIntent.Id, stripeEventId);
                throw;
            }
        }

        public async Task HandlePaymentIntentFailedAsync(PaymentIntent paymentIntent, string stripeEventId, CancellationToken cancellationToken = default)
        {
            Log.Information(
                "Processing payment_intent.payment_failed: PaymentIntentId={PaymentIntentId}, EventId={EventId}",
                paymentIntent.Id, stripeEventId);

            // Find payment by TransactionId
            var payments = await _unitOfWork.Payments.FindAsync(p => p.TransactionId == paymentIntent.Id);
            var payment = payments.FirstOrDefault();

            if (payment == null)
            {
                Log.Warning("Payment not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
                return;
            }

            // Idempotency check
            if (payment.StripeEventId == stripeEventId)
            {
                Log.Information(
                    "Event already processed: PaymentId={PaymentId}, EventId={EventId}",
                    payment.Id, stripeEventId);
                return;
            }

            if (PaymentStatusTransitionValidator.IsTerminalStatus(payment.Status))
            {
                Log.Information(
                    "Payment already in terminal state: PaymentId={PaymentId}, Status={Status}, EventId={EventId}",
                    payment.Id, payment.Status, stripeEventId);
                return;
            }

            // Validate status transition
            PaymentStatusTransitionValidator.ValidateTransition(payment.Status, PaymentStatus.Failed, payment.Id);

            // Extract failure reason from payment intent
            var failureReason = paymentIntent.LastPaymentError?.Message ?? "Payment failed";

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var oldStatus = payment.Status;
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = failureReason;
                payment.StripeEventId = stripeEventId;
                await _unitOfWork.Payments.UpdateAsync(payment);

                await _unitOfWork.CommitTransactionAsync();

                Log.Information(
                    "Payment status updated to Failed: PaymentId={PaymentId}, OldStatus={OldStatus}, FailureReason={FailureReason}, EventId={EventId}",
                    payment.Id, oldStatus, failureReason, stripeEventId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex,
                    "Error processing payment_intent.payment_failed: PaymentIntentId={PaymentIntentId}, EventId={EventId}",
                    paymentIntent.Id, stripeEventId);
                throw;
            }
        }

        public async Task HandlePaymentIntentCanceledAsync(PaymentIntent paymentIntent, string stripeEventId, CancellationToken cancellationToken = default)
        {
            Log.Information(
                "Processing payment_intent.canceled: PaymentIntentId={PaymentIntentId}, EventId={EventId}",
                paymentIntent.Id, stripeEventId);

            // Find payment by TransactionId
            var payments = await _unitOfWork.Payments.FindAsync(p => p.TransactionId == paymentIntent.Id);
            var payment = payments.FirstOrDefault();

            if (payment == null)
            {
                Log.Warning("Payment not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
                return;
            }

            // Idempotency check
            if (payment.StripeEventId == stripeEventId)
            {
                Log.Information(
                    "Event already processed: PaymentId={PaymentId}, EventId={EventId}",
                    payment.Id, stripeEventId);
                return;
            }

            if (PaymentStatusTransitionValidator.IsTerminalStatus(payment.Status))
            {
                Log.Information(
                    "Payment already in terminal state: PaymentId={PaymentId}, Status={Status}, EventId={EventId}",
                    payment.Id, payment.Status, stripeEventId);
                return;
            }

            // Validate status transition
            PaymentStatusTransitionValidator.ValidateTransition(payment.Status, PaymentStatus.Cancelled, payment.Id);

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var oldStatus = payment.Status;
                payment.Status = PaymentStatus.Cancelled;
                payment.StripeEventId = stripeEventId;
                await _unitOfWork.Payments.UpdateAsync(payment);

                // Optionally update booking status if payment is cancelled
                var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId, cancellationToken);
                if (booking != null && booking.Status == BookingStatus.Pending)
                {
                    booking.Status = BookingStatus.Cancelled;
                    booking.CancellationReason = "Payment was cancelled";
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                    Log.Information(
                        "Booking cancelled due to payment cancellation: BookingId={BookingId}",
                        booking.Id);
                }

                await _unitOfWork.CommitTransactionAsync();

                Log.Information(
                    "Payment status updated to Cancelled: PaymentId={PaymentId}, OldStatus={OldStatus}, EventId={EventId}",
                    payment.Id, oldStatus, stripeEventId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex,
                    "Error processing payment_intent.canceled: PaymentIntentId={PaymentIntentId}, EventId={EventId}",
                    paymentIntent.Id, stripeEventId);
                throw;
            }
        }

        public async Task HandleChargeRefundedAsync(Charge charge, string stripeEventId, CancellationToken cancellationToken = default)
        {
            Log.Information(
                "Processing charge.refunded: ChargeId={ChargeId}, EventId={EventId}",
                charge.Id, stripeEventId);

            // Find payment by TransactionId (PaymentIntent Id from charge)
            var paymentIntentId = charge.PaymentIntentId;
            if (string.IsNullOrEmpty(paymentIntentId))
            {
                Log.Warning("Charge has no PaymentIntentId: ChargeId={ChargeId}", charge.Id);
                return;
            }

            var payments = await _unitOfWork.Payments.FindAsync(p => p.TransactionId == paymentIntentId);
            var payment = payments.FirstOrDefault();

            if (payment == null)
            {
                Log.Warning("Payment not found for PaymentIntent: {PaymentIntentId}", paymentIntentId);
                return;
            }

            // Only process refunds for completed payments
            if (payment.Status != PaymentStatus.Completed)
            {
                Log.Warning(
                    "Cannot refund payment that is not completed: PaymentId={PaymentId}, Status={Status}",
                    payment.Id, payment.Status);
                return;
            }

            // Idempotency check - for refunds, we might process multiple events
            // So we check if this specific event was already processed
            if (payment.StripeEventId == stripeEventId)
            {
                Log.Information(
                    "Event already processed: PaymentId={PaymentId}, EventId={EventId}",
                    payment.Id, stripeEventId);
                return;
            }

            // Validate status transition
            PaymentStatusTransitionValidator.ValidateTransition(payment.Status, PaymentStatus.Refunded, payment.Id);

            // Calculate refund amount
            var refundAmount = charge.AmountRefunded / 100m;
            PaymentValidationService.ValidateRefundAmount(charge, payment, refundAmount);

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var oldStatus = payment.Status;
                payment.Status = PaymentStatus.Refunded;
                payment.StripeEventId = stripeEventId;
                await _unitOfWork.Payments.UpdateAsync(payment);

                // Optionally update booking status if refunded
                var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId, cancellationToken);
                if (booking != null && booking.Status == BookingStatus.Confirmed)
                {
                    booking.Status = BookingStatus.Cancelled;
                    booking.CancellationReason = $"Payment refunded: {refundAmount} {payment.Currency}";
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                    Log.Information(
                        "Booking cancelled due to refund: BookingId={BookingId}, RefundAmount={RefundAmount}",
                        booking.Id, refundAmount);
                }

                await _unitOfWork.CommitTransactionAsync();

                Log.Information(
                    "Payment status updated to Refunded: PaymentId={PaymentId}, OldStatus={OldStatus}, RefundAmount={RefundAmount}, EventId={EventId}",
                    payment.Id, oldStatus, refundAmount, stripeEventId);

                // Invalidate admin dashboard caches
                try
                {
                    await _cacheInvalidator.RemoveByPrefixAsync(Infrastructure.Caching.CacheKeys.Admin.Prefix, cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error invalidating cache for PaymentId: {PaymentId}", payment.Id);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex,
                    "Error processing charge.refunded: ChargeId={ChargeId}, EventId={EventId}",
                    charge.Id, stripeEventId);
                throw;
            }
        }
    }
}

