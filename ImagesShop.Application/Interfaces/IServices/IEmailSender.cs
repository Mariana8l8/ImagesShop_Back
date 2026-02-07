namespace ImagesShop.Application.Interfaces.IServices
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
    }
}
