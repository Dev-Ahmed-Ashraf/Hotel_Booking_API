using Hotel_Booking_API.Domain.Enums;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Payments.Services
{
    /// <summary>
    /// Validates payment status transitions according to business rules.
    /// </summary>
    public static class PaymentStatusTransitionValidator
    {
        /// <summary>
        /// Validates if a payment status transition is allowed.
        /// </summary>
        /// <param name="currentStatus">Current payment status</param>
        /// <param name="newStatus">New payment status to transition to</param>
        /// <returns>True if transition is valid, false otherwise</returns>
        public static bool IsValidTransition(PaymentStatus currentStatus, PaymentStatus newStatus)
        {
            // Same status is always valid (idempotency)
            if (currentStatus == newStatus)
                return true;

            return currentStatus switch
            {
                PaymentStatus.Pending =>
                    newStatus is PaymentStatus.Completed or PaymentStatus.Failed or PaymentStatus.Cancelled,

                PaymentStatus.Completed =>
                    newStatus is PaymentStatus.Refunded,

                // Terminal states - no transitions allowed
                PaymentStatus.Failed => false,
                PaymentStatus.Refunded => false,
                PaymentStatus.Cancelled => false,

                _ => false
            };
        }

        /// <summary>
        /// Validates a payment status transition and throws an exception if invalid.
        /// </summary>
        /// <param name="currentStatus">Current payment status</param>
        /// <param name="newStatus">New payment status to transition to</param>
        /// <param name="paymentId">Payment ID for logging</param>
        /// <exception cref="InvalidOperationException">Thrown when transition is invalid</exception>
        public static void ValidateTransition(PaymentStatus currentStatus, PaymentStatus newStatus, int paymentId)
        {
            if (!IsValidTransition(currentStatus, newStatus))
            {
                Log.Warning(
                    "Invalid payment status transition attempted: PaymentId={PaymentId}, CurrentStatus={CurrentStatus}, NewStatus={NewStatus}",
                    paymentId, currentStatus, newStatus);

                throw new InvalidOperationException(
                    $"Invalid payment status transition from '{currentStatus}' to '{newStatus}' for payment {paymentId}.");
            }

            Log.Debug(
                "Payment status transition validated: PaymentId={PaymentId}, CurrentStatus={CurrentStatus}, NewStatus={NewStatus}",
                paymentId, currentStatus, newStatus);
        }

        /// <summary>
        /// Checks if a payment status is a terminal state (no further transitions allowed).
        /// </summary>
        /// <param name="status">Payment status to check</param>
        /// <returns>True if status is terminal, false otherwise</returns>
        public static bool IsTerminalStatus(PaymentStatus status)
        {
            return status is PaymentStatus.Failed or PaymentStatus.Refunded or PaymentStatus.Cancelled;
        }
    }
}

