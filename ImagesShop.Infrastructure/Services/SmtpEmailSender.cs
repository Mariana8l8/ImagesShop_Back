using System.Net;
using System.Net.Mail;
using ImagesShop.Application;
using ImagesShop.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImagesShop.Infrastructure.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSenderOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSenderOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
                throw new InvalidOperationException("EmailSender:Host is not configured.");
            if (string.IsNullOrWhiteSpace(_options.FromEmail))
                throw new InvalidOperationException("EmailSender:FromEmail is not configured.");
            if (string.IsNullOrWhiteSpace(_options.UserName))
                throw new InvalidOperationException("EmailSender:UserName is not configured.");
            if (string.IsNullOrWhiteSpace(_options.Password))
                throw new InvalidOperationException("EmailSender:Password is not configured.");

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_options.UserName, _options.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, string.IsNullOrWhiteSpace(_options.FromName) ? null : _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            _logger.LogInformation(
                "Sending email to {To} via SMTP host {Host}:{Port} as {User}",
                toEmail,
                _options.Host,
                _options.Port,
                _options.UserName);

            cancellationToken.ThrowIfCancellationRequested();
            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
