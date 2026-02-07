using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IRepositories
{
    public interface IEmailVerificationCodeRepository
    {
        Task AddAsync(EmailVerificationCode code, CancellationToken cancellationToken = default);

        Task AddForEmailAsync(string email, EmailVerificationCode code, CancellationToken cancellationToken = default);

        Task<EmailVerificationCode?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<EmailVerificationCode?> GetLatestActiveByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task MarkUsedAsync(EmailVerificationCode code, CancellationToken cancellationToken = default);
    }
}
