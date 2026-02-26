using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class UserTransactionRepository : IUserTransactionRepository
    {
        private readonly AppDbContext _appDbContext;

        public UserTransactionRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<UserTransaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.UserTransactions
                .AsNoTracking()
                .Where(userTransaction => userTransaction.UserId == userId)
                .OrderByDescending(userTransaction => userTransaction.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserTransaction userTransaction, CancellationToken cancellationToken = default)
        {
            await _appDbContext.UserTransactions.AddAsync(userTransaction, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
