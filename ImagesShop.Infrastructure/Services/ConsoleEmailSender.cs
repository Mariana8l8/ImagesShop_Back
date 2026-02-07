using ImagesShop.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace ImagesShop.Infrastructure.Services
{
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;

        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("EMAIL to {To}. Subject: {Subject}. Body: {Body}", toEmail, subject, body);
            return Task.CompletedTask;
        }
    }
}
