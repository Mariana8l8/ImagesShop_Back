using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class EmailVerificationCodeRepository : IEmailVerificationCodeRepository
    {
        private readonly AppDbContext _appDbContext;

        public EmailVerificationCodeRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(EmailVerificationCode emailVerificationCode, CancellationToken cancellationToken = default)
        {
            await _appDbContext.EmailVerificationCodes.AddAsync(emailVerificationCode, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddForEmailAsync(string email, EmailVerificationCode emailVerificationCode, CancellationToken cancellationToken = default)
        {
            emailVerificationCode.Email = email;
            await _appDbContext.EmailVerificationCodes.AddAsync(emailVerificationCode, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<EmailVerificationCode?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _appDbContext.EmailVerificationCodes
                .Where(verificationCode => verificationCode.Email == email)
                .OrderByDescending(verificationCode => verificationCode.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<EmailVerificationCode?> GetLatestActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _appDbContext.EmailVerificationCodes
                .Where(verificationCode => verificationCode.Email == email && 
                                           verificationCode.UsedAt == null && 
                                           verificationCode.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(verificationCode => verificationCode.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task MarkUsedAsync(EmailVerificationCode emailVerificationCode, CancellationToken cancellationToken = default)
        {
            emailVerificationCode.UsedAt = DateTime.UtcNow;
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
