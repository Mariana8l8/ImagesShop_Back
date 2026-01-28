using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class PurchaseHistoryRepository : IPurchaseHistoryRepository
    {
        private readonly AppDbContext _database;

        public PurchaseHistoryRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<PurchaseHistory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _database.Purchases
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<PurchaseHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _database.Purchases
                .AsNoTracking()
                .FirstOrDefaultAsync(purchaseHistory => purchaseHistory.Id == id, cancellationToken);
        }

        public async Task AddAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default)
        {
            await _database.Purchases.AddAsync(purchaseHistory, cancellationToken);
        }

        public Task UpdateAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default)
        {
            _database.Purchases.Update(purchaseHistory);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _database.Purchases.FindAsync(new object[] { id }, cancellationToken);
            if (entity is not null)
            {
                _database.Purchases.Remove(entity);
            }
        }
    }
}
