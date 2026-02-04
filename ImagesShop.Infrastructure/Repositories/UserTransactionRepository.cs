using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class UserTransactionRepository : IUserTransactionRepository
    {
        private readonly AppDbContext _db;

        public UserTransactionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UserTransaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.UserTransactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserTransaction transaction, CancellationToken cancellationToken = default)
        {
            await _db.UserTransactions.AddAsync(transaction, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
