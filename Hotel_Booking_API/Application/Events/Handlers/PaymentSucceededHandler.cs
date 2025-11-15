using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Services;
using Hotel_Booking_API.Infrastructure.Templates;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Events.Handlers
{
    public class PaymentSucceededHandler : INotificationHandler<PaymentSucceededEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentSucceededHandler(IEmailService emailService, IUnitOfWork unitOfWork)
        {
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PaymentSucceededEvent notification, CancellationToken cancellationToken)
        {
            Log.Information(
                "Processing PaymentSucceededEvent for PaymentId: {PaymentId}, BookingId: {BookingId}, UserId: {UserId}",
                notification.PaymentId, notification.BookingId, notification.UserId);

            try
            {
                // Load user details
                var user = await _unitOfWork.Users.GetByIdAsync(notification.UserId, cancellationToken);
                if (user == null)
                {
                    Log.Warning(
                        "User {UserId} not found for payment confirmation email: PaymentId={PaymentId}",
                        notification.UserId, notification.PaymentId);
                    return;
                }

                // Load booking details
                var booking = await _unitOfWork.Bookings.GetByIdAsync(notification.BookingId, cancellationToken);
                if (booking == null)
                {
                    Log.Warning(
                        "Booking {BookingId} not found for payment confirmation email: PaymentId={PaymentId}",
                        notification.BookingId, notification.PaymentId);
                    return;
                }

                // Load email template
                var template = EmailTemplateLoader.LoadTemplate("PaymentSuccessTemplate.html");

                // Prepare placeholders
                var placeholders = new Dictionary<string, string>
                {
                    { "UserName", $"{user.FirstName} {user.LastName}" },
                    { "BookingId", booking.Id.ToString() },
                    { "Amount", notification.Amount.ToString("F2") },
                    { "PaymentDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") },
                    { "TransactionId", notification.TransactionId }
                };

                // Fill template with data
                var htmlBody = EmailTemplateLoader.FillTemplate(template, placeholders);

                // Send email (non-blocking, with retry logic)
                var subject = $"Booking Confirmation - Payment Successful #{booking.Id}";
                
                // Use fire-and-forget pattern with retry
                _ = Task.Run(async () =>
                {
                    const int maxRetries = 3;
                    for (int attempt = 1; attempt <= maxRetries; attempt++)
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(user.Email, subject, htmlBody);
                            Log.Information(
                                "Successfully sent payment confirmation email to {Email} for PaymentId: {PaymentId}",
                                user.Email, notification.PaymentId);
                            return;
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex,
                                "Failed to send payment confirmation email (attempt {Attempt}/{MaxRetries}): Email={Email}, PaymentId={PaymentId}",
                                attempt, maxRetries, user.Email, notification.PaymentId);

                            if (attempt < maxRetries)
                            {
                                // Exponential backoff: wait 2^attempt seconds before retry
                                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                            }
                            else
                            {
                                Log.Error(ex,
                                    "Failed to send payment confirmation email after {MaxRetries} attempts: Email={Email}, PaymentId={PaymentId}",
                                    maxRetries, user.Email, notification.PaymentId);
                            }
                        }
                    }
                }, cancellationToken);

                Log.Information(
                    "Payment confirmation email queued for {Email} (PaymentId: {PaymentId})",
                    user.Email, notification.PaymentId);
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Error processing PaymentSucceededEvent for PaymentId: {PaymentId}, BookingId: {BookingId}",
                    notification.PaymentId, notification.BookingId);
                // Don't rethrow - email failures should not block payment processing
            }
        }
    }
}

