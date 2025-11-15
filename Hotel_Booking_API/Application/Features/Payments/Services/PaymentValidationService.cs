using Hotel_Booking_API.Domain.Entities;
using Serilog;
using Stripe;

namespace Hotel_Booking_API.Application.Features.Payments.Services
{
    /// <summary>
    /// Validates payment data between Stripe and database to ensure consistency.
    /// </summary>
    public static class PaymentValidationService
    {
        /// <summary>
        /// Validates that the Stripe PaymentIntent amount matches the payment amount in the database.
        /// </summary>
        /// <param name="paymentIntent">Stripe PaymentIntent</param>
        /// <param name="payment">Payment entity from database</param>
        /// <exception cref="InvalidOperationException">Thrown when amounts don't match</exception>
        public static void ValidateAmount(PaymentIntent paymentIntent, Payment payment)
        {
            // Stripe stores amount in smallest currency unit (cents for USD)
            var stripeAmount = paymentIntent.Amount / 100m;
            var dbAmount = payment.Amount;

            if (Math.Abs(stripeAmount - dbAmount) > 0.01m) // Allow small rounding differences
            {
                Log.Warning(
                    "Amount mismatch detected: PaymentId={PaymentId}, StripeAmount={StripeAmount}, DbAmount={DbAmount}",
                    payment.Id, stripeAmount, dbAmount);

                throw new InvalidOperationException(
                    $"Payment amount mismatch: Stripe amount {stripeAmount} does not match database amount {dbAmount} for payment {payment.Id}.");
            }

            Log.Debug(
                "Amount validation passed: PaymentId={PaymentId}, Amount={Amount}",
                payment.Id, dbAmount);
        }

        /// <summary>
        /// Validates that the Stripe PaymentIntent currency matches the payment currency in the database.
        /// </summary>
        /// <param name="paymentIntent">Stripe PaymentIntent</param>
        /// <param name="payment">Payment entity from database</param>
        /// <exception cref="InvalidOperationException">Thrown when currencies don't match</exception>
        public static void ValidateCurrency(PaymentIntent paymentIntent, Payment payment)
        {
            var stripeCurrency = paymentIntent.Currency?.ToLowerInvariant() ?? "usd";
            var dbCurrency = payment.Currency?.ToLowerInvariant() ?? "usd";

            if (stripeCurrency != dbCurrency)
            {
                Log.Warning(
                    "Currency mismatch detected: PaymentId={PaymentId}, StripeCurrency={StripeCurrency}, DbCurrency={DbCurrency}",
                    payment.Id, stripeCurrency, dbCurrency);

                throw new InvalidOperationException(
                    $"Payment currency mismatch: Stripe currency '{stripeCurrency}' does not match database currency '{dbCurrency}' for payment {payment.Id}.");
            }

            Log.Debug(
                "Currency validation passed: PaymentId={PaymentId}, Currency={Currency}",
                payment.Id, dbCurrency);
        }

        /// <summary>
        /// Validates both amount and currency of a PaymentIntent against the payment entity.
        /// </summary>
        /// <param name="paymentIntent">Stripe PaymentIntent</param>
        /// <param name="payment">Payment entity from database</param>
        /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
        public static void ValidatePaymentIntent(PaymentIntent paymentIntent, Payment payment)
        {
            ValidateAmount(paymentIntent, payment);
            ValidateCurrency(paymentIntent, payment);
        }

        /// <summary>
        /// Validates that a Stripe Charge amount matches the payment amount in the database.
        /// Used for refund validation.
        /// </summary>
        /// <param name="charge">Stripe Charge</param>
        /// <param name="payment">Payment entity from database</param>
        /// <param name="refundAmount">The refund amount being processed</param>
        /// <exception cref="InvalidOperationException">Thrown when refund amount is invalid</exception>
        public static void ValidateRefundAmount(Charge charge, Payment payment, decimal refundAmount)
        {
            // Stripe stores amount in smallest currency unit (cents for USD)
            var chargeAmount = charge.Amount / 100m;
            var paymentAmount = payment.Amount;

            if (refundAmount <= 0)
            {
                throw new InvalidOperationException(
                    $"Refund amount must be greater than zero for payment {payment.Id}.");
            }

            if (refundAmount > paymentAmount)
            {
                Log.Warning(
                    "Refund amount exceeds payment amount: PaymentId={PaymentId}, RefundAmount={RefundAmount}, PaymentAmount={PaymentAmount}",
                    payment.Id, refundAmount, paymentAmount);

                throw new InvalidOperationException(
                    $"Refund amount {refundAmount} cannot exceed payment amount {paymentAmount} for payment {payment.Id}.");
            }

            // Validate charge amount matches payment amount
            if (Math.Abs(chargeAmount - paymentAmount) > 0.01m)
            {
                Log.Warning(
                    "Charge amount mismatch: PaymentId={PaymentId}, ChargeAmount={ChargeAmount}, PaymentAmount={PaymentAmount}",
                    payment.Id, chargeAmount, paymentAmount);

                throw new InvalidOperationException(
                    $"Charge amount {chargeAmount} does not match payment amount {paymentAmount} for payment {payment.Id}.");
            }

            Log.Debug(
                "Refund amount validation passed: PaymentId={PaymentId}, RefundAmount={RefundAmount}",
                payment.Id, refundAmount);
        }
    }
}

