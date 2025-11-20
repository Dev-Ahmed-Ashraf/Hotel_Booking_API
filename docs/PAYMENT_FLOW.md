# Payment System Flow Documentation

## Overview

This document describes the complete payment flow in the Hotel Booking API, including integration with Stripe, webhook handling, and database updates.

## Architecture

The payment system follows Clean Architecture principles with CQRS pattern:
- **Domain Layer**: Payment entity, PaymentStatus enum
- **Application Layer**: Commands, Queries, Services, Events
- **Infrastructure Layer**: StripeService, PaymentUpdateService
- **API Layer**: Controllers (PaymentsController, StripeWebhooksController)

## Payment Flow

### 1. Create Payment Intent

**Endpoint**: `POST /api/payments/intents`

**Handler**: `CreatePaymentIntentHandler`

**Flow**:
1. User creates a booking (status: Pending)
2. User requests payment intent creation
3. Handler validates:
   - Booking exists and is in Pending status
   - Booking total price > 0
   - Payment not already completed
4. Handler creates Stripe PaymentIntent via `StripeService`
5. Handler creates/updates Payment entity:
   - Status: Pending
   - Amount: Booking.TotalPrice
   - Currency: From configuration (default: "usd")
   - TransactionId: Stripe PaymentIntent ID
6. Returns PaymentIntent ID and ClientSecret to frontend

**Key Points**:
- Idempotency key: `booking-{bookingId}-v1`
- Currency is stored in Payment entity for validation
- Payment entity is created before Stripe PaymentIntent to ensure consistency

### 2. Stripe Checkout

**Frontend Flow**:
1. Frontend receives PaymentIntent ID and ClientSecret
2. Uses Stripe.js to collect payment method
3. Confirms payment with Stripe
4. Stripe processes payment asynchronously

### 3. Webhook Processing

**Endpoint**: `POST /api/stripe/webhook`

**Controller**: `StripeWebhooksController`

**Flow**:
1. Stripe sends webhook event to endpoint
2. Controller validates webhook signature
3. Controller returns 200 OK immediately (async processing)
4. Event is processed asynchronously by `PaymentUpdateService`

**Supported Events**:
- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `payment_intent.canceled`
- `charge.refunded`

### 4. Payment Update Service

**Service**: `PaymentUpdateService`

**Responsibilities**:
- Idempotency checking (by StripeEventId)
- Amount/currency validation
- Payment status transition validation
- Transaction safety (database transactions)
- Event publishing (after successful commit)
- Cache invalidation

**Event Handlers**:

#### 4.1 Payment Intent Succeeded

**Handler**: `HandlePaymentIntentSucceededAsync`

**Flow**:
1. Find payment by TransactionId (PaymentIntent ID)
2. Check idempotency (StripeEventId, terminal status)
3. Validate status transition (Pending → Completed)
4. Validate amount and currency match
5. Begin database transaction
6. Update payment:
   - Status: Completed
   - PaidAt: DateTime.UtcNow
   - StripeEventId: Event ID
7. Update booking (if Pending → Confirmed)
8. Commit transaction
9. Publish `PaymentSucceededEvent` (fire-and-forget)
10. Invalidate admin dashboard cache

**Business Rules**:
- Only Pending payments can transition to Completed
- Booking status changes from Pending to Confirmed
- Event is published only after successful DB commit

#### 4.2 Payment Intent Failed

**Handler**: `HandlePaymentIntentFailedAsync`

**Flow**:
1. Find payment by TransactionId
2. Check idempotency
3. Validate status transition (Pending → Failed)
4. Extract failure reason from PaymentIntent
5. Begin transaction
6. Update payment:
   - Status: Failed
   - FailureReason: Error message
   - StripeEventId: Event ID
7. Commit transaction

**Business Rules**:
- Only Pending payments can transition to Failed
- Failure reason is stored for debugging

#### 4.3 Payment Intent Canceled

**Handler**: `HandlePaymentIntentCanceledAsync`

**Flow**:
1. Find payment by TransactionId
2. Check idempotency
3. Validate status transition (Pending → Cancelled)
4. Begin transaction
5. Update payment:
   - Status: Cancelled
   - StripeEventId: Event ID
6. Update booking (if Pending → Cancelled with reason)
7. Commit transaction

**Business Rules**:
- Only Pending payments can transition to Cancelled
- Booking is cancelled if payment is cancelled

#### 4.4 Charge Refunded

**Handler**: `HandleChargeRefundedAsync`

**Flow**:
1. Find payment by PaymentIntent ID from Charge
2. Validate payment is Completed
3. Check idempotency
4. Validate status transition (Completed → Refunded)
5. Validate refund amount
6. Begin transaction
7. Update payment:
   - Status: Refunded
   - StripeEventId: Event ID
8. Update booking (if Confirmed → Cancelled with refund reason)
9. Commit transaction
10. Invalidate admin dashboard cache

**Business Rules**:
- Only Completed payments can be refunded
- Refund amount cannot exceed payment amount
- Booking is cancelled when payment is refunded

### 5. Email Notification

**Event**: `PaymentSucceededEvent`

**Handler**: `PaymentSucceededHandler`

**Flow**:
1. Event is published after successful payment commit
2. Handler loads user and booking details
3. Loads email template
4. Sends email asynchronously (fire-and-forget)
5. Retry logic: 3 attempts with exponential backoff

**Key Points**:
- Email sending is non-blocking
- Retry logic handles transient failures
- Email failures don't affect payment processing

## Payment Status Transitions

### Valid Transitions

```
Pending → Completed
Pending → Failed
Pending → Cancelled
Completed → Refunded
```

### Terminal States

- **Failed**: No further transitions allowed
- **Refunded**: No further transitions allowed
- **Cancelled**: No further transitions allowed

### Validation

Status transitions are validated by `PaymentStatusTransitionValidator`:
- Prevents invalid transitions
- Logs warnings for invalid attempts
- Throws exceptions for business rule violations

## Idempotency

### Strategy

1. **StripeEventId Tracking**: Each processed event stores its StripeEventId in Payment entity
2. **Terminal Status Check**: Payments in terminal states are not updated
3. **Event ID Check**: Events with same StripeEventId are skipped

### Implementation

```csharp
// Check if event already processed
if (payment.StripeEventId == stripeEventId)
{
    return; // Already processed
}

// Check if payment is in terminal state
if (PaymentStatusTransitionValidator.IsTerminalStatus(payment.Status))
{
    return; // Cannot update terminal state
}
```

## Validation

### Amount Validation

- Stripe stores amounts in smallest currency unit (cents for USD)
- Database stores amounts as decimal
- Validation converts Stripe amount: `amount / 100m`
- Allows 0.01 difference for rounding

### Currency Validation

- Stripe PaymentIntent currency must match Payment.Currency
- Case-insensitive comparison
- Default currency: "usd"

### Implementation

`PaymentValidationService` provides:
- `ValidateAmount()`: Validates amount match
- `ValidateCurrency()`: Validates currency match
- `ValidatePaymentIntent()`: Validates both
- `ValidateRefundAmount()`: Validates refund amount

## Transaction Safety

### Database Transactions

All payment updates use database transactions:

```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    // Update payment and booking
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

### Benefits

- Atomic updates (payment + booking)
- Rollback on errors
- Data consistency

## Error Handling

### Webhook Errors

- Invalid signature → 401 Unauthorized
- Missing signature → 400 Bad Request
- Processing errors → Logged, webhook returns 200 OK (Stripe retries)

### Payment Update Errors

- Validation errors → Logged, exception thrown
- Database errors → Transaction rolled back, exception thrown
- Event publishing errors → Logged, don't fail webhook

### Email Errors

- Retry logic: 3 attempts with exponential backoff
- Final failure → Logged, doesn't affect payment

## Logging

### Structured Logging

All operations use structured logging with Serilog:

```csharp
Log.Information(
    "Payment status updated: PaymentId={PaymentId}, OldStatus={OldStatus}, NewStatus={NewStatus}",
    payment.Id, oldStatus, newStatus);
```

### Log Levels

- **Information**: Successful operations, status changes
- **Warning**: Validation failures, missing data
- **Error**: Exceptions, processing failures
- **Debug**: Detailed operation information

## Cache Invalidation

After payment updates:
- Admin dashboard cache is invalidated
- Ensures accurate statistics
- Uses `ICacheInvalidator.RemoveByPrefixAsync()`

## Testing

### Unit Tests

Test:
- Status transition validation
- Amount/currency validation
- Idempotency checks

### Integration Tests

Test:
- Webhook processing
- Database transactions
- Event publishing

### Manual Testing

1. Create booking
2. Create payment intent
3. Use Stripe test cards
4. Verify webhook processing
5. Check email delivery

## Configuration

### Stripe Configuration

```json
{
  "Stripe": {
    "ApiKey": "sk_test_...",
    "WebhookSecret": "whsec_...",
    "Currency": "usd"
  }
}
```

### Webhook Endpoint

Configure in Stripe Dashboard:
- URL: `https://your-domain.com/api/stripe/webhook`
- Events: `payment_intent.succeeded`, `payment_intent.payment_failed`, `payment_intent.canceled`, `charge.refunded`

## Security

### Webhook Signature Verification

- All webhooks are verified using Stripe signature
- Invalid signatures are rejected
- Uses `StripeService.VerifyWebhookSignature()`

### Transaction Safety

- All updates use database transactions
- Prevents partial updates
- Ensures data consistency

## Performance

### Async Processing

- Webhook returns 200 OK immediately
- Processing happens asynchronously
- Prevents Stripe timeout issues

### Fire-and-Forget

- Email sending is fire-and-forget
- Event publishing is non-blocking
- Cache invalidation is non-blocking

## Monitoring

### Key Metrics

- Payment success rate
- Webhook processing time
- Email delivery rate
- Failed payment rate

### Alerts

- Webhook signature failures
- Payment validation errors
- Database transaction failures
- Email delivery failures

## Troubleshooting

### Common Issues

1. **Webhook not received**
   - Check Stripe webhook configuration
   - Verify endpoint URL
   - Check firewall rules

2. **Payment not updating**
   - Check webhook logs
   - Verify StripeEventId tracking
   - Check status transition validation

3. **Email not sent**
   - Check email service configuration
   - Review retry logs
   - Verify event publishing

4. **Amount mismatch**
   - Check currency configuration
   - Verify amount conversion (cents to decimal)
   - Review validation logs

## Future Improvements

1. **Partial Refunds**: Support partial refunds
2. **Payment Methods**: Support multiple payment methods
3. **Retry Queue**: Implement retry queue for failed webhooks
4. **Webhook Replay**: Support webhook event replay
5. **Analytics**: Enhanced payment analytics

