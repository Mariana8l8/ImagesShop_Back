using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class PendingRegistrationRepository : IPendingRegistrationRepository
    {
        private readonly AppDbContext _db;

        public PendingRegistrationRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<PendingRegistration?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _db.PendingRegistrations.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }

        public async Task UpsertAsync(PendingRegistration pending, CancellationToken cancellationToken = default)
        {
            var existing = await _db.PendingRegistrations.FirstOrDefaultAsync(x => x.Email == pending.Email, cancellationToken);
            if (existing is null)
            {
                await _db.PendingRegistrations.AddAsync(pending, cancellationToken);
            }
            else
            {
                existing.Name = pending.Name;
                existing.PasswordHash = pending.PasswordHash;
                existing.PasswordSalt = pending.PasswordSalt;
                existing.CreatedAt = pending.CreatedAt;
                existing.ExpiresAt = pending.ExpiresAt;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(PendingRegistration pending, CancellationToken cancellationToken = default)
        {
            _db.PendingRegistrations.Remove(pending);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
