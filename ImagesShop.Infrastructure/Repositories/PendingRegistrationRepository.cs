using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class PendingRegistrationRepository : IPendingRegistrationRepository
    {
        private readonly AppDbContext _appDbContext;

        public PendingRegistrationRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Task<PendingRegistration?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _appDbContext.PendingRegistrations.FirstOrDefaultAsync(pendingRegistration => pendingRegistration.Email == email, cancellationToken);
        }

        public async Task UpsertAsync(PendingRegistration pendingRegistration, CancellationToken cancellationToken = default)
        {
            var existingPendingRegistration = await _appDbContext.PendingRegistrations
                .FirstOrDefaultAsync(item => item.Email == pendingRegistration.Email, cancellationToken);
            
            if (existingPendingRegistration is null)
            {
                await _appDbContext.PendingRegistrations.AddAsync(pendingRegistration, cancellationToken);
            }
            else
            {
                existingPendingRegistration.Name = pendingRegistration.Name;
                existingPendingRegistration.PasswordHash = pendingRegistration.PasswordHash;
                existingPendingRegistration.PasswordSalt = pendingRegistration.PasswordSalt;
                existingPendingRegistration.CreatedAt = pendingRegistration.CreatedAt;
                existingPendingRegistration.ExpiresAt = pendingRegistration.ExpiresAt;
            }

            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(PendingRegistration pendingRegistration, CancellationToken cancellationToken = default)
        {
            _appDbContext.PendingRegistrations.Remove(pendingRegistration);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
