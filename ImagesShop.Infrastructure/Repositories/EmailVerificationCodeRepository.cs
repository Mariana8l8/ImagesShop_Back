using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class EmailVerificationCodeRepository : IEmailVerificationCodeRepository
    {
        private readonly AppDbContext _db;

        public EmailVerificationCodeRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(EmailVerificationCode code, CancellationToken cancellationToken = default)
        {
            await _db.EmailVerificationCodes.AddAsync(code, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task AddForEmailAsync(string email, EmailVerificationCode code, CancellationToken cancellationToken = default)
        {
            code.Email = email;
            await _db.EmailVerificationCodes.AddAsync(code, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public Task<EmailVerificationCode?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _db.EmailVerificationCodes
                .Where(x => x.Email == email)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<EmailVerificationCode?> GetLatestActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _db.EmailVerificationCodes
                .Where(x => x.Email == email && x.UsedAt == null && x.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task MarkUsedAsync(EmailVerificationCode code, CancellationToken cancellationToken = default)
        {
            code.UsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
