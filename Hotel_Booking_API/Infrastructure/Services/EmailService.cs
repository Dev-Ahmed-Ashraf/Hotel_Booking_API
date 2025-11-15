using Microsoft.Extensions.Options;
using Serilog;
using System.Net;
using System.Net.Mail;

namespace Hotel_Booking_API.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            Log.Information("Starting to send email to {Email}", to);

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
                    To = { new MailAddress(to) },
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    EnableSsl = _smtpSettings.EnableSsl,
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
                };

                await client.SendMailAsync(message);

                Log.Information("Successfully sent email to {Email} with subject: {Subject}", to, subject);
            }
            catch (SmtpException ex)
            {
                Log.Error(ex, "SMTP error while sending email to {Email}: {Error}", to, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while sending email to {Email}: {Error}", to, ex.Message);
                throw;
            }
        }
    }
}

