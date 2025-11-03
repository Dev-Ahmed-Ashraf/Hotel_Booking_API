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
            Log.Information("Processing PaymentSucceededEvent for PaymentId: {PaymentId}, UserId: {UserId}", 
                notification.PaymentId, notification.UserId);

            try
            {
                // Load user details
                var user = await _unitOfWork.Users.GetByIdAsync(notification.UserId, cancellationToken);
                if (user == null)
                {
                    Log.Warning("User {UserId} not found for payment confirmation email", notification.UserId);
                    return;
                }

                // Load booking details
                var booking = await _unitOfWork.Bookings.GetByIdAsync(notification.BookingId, cancellationToken);
                if (booking == null)
                {
                    Log.Warning("Booking {BookingId} not found for payment confirmation email", notification.BookingId);
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

                // Send email
                var subject = $"Booking Confirmation - Payment Successful #{booking.Id}";
                await _emailService.SendEmailAsync(user.Email, subject, htmlBody);

                Log.Information("Successfully sent payment confirmation email to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing PaymentSucceededEvent for PaymentId: {PaymentId}", 
                    notification.PaymentId);
                // Don't rethrow - email failures should not block payment processing
            }
        }
    }
}

