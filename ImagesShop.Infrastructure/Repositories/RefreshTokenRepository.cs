using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _database;

        public RefreshTokenRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            await _database.RefreshTokens.AddAsync(token, cancellationToken);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _database.RefreshTokens
                .Include(refreshToken => refreshToken.User)
                .FirstOrDefaultAsync(refreshToken => refreshToken.Token == token, cancellationToken);
        }

        public async Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            token.RevokedAt = DateTime.UtcNow;
            _database.RefreshTokens.Update(token);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _database.SaveChangesAsync(cancellationToken);
        }
    }
}