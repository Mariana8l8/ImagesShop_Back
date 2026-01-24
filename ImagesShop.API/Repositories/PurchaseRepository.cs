using ImagesShop.API.Data;
using ImagesShop.API.Models;
using ImagesShop.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.API.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly AppDbContext _database;

        public PurchaseRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<PurchaseHistory>> GetAllAsync(CancellationToken ct = default)
        {
            return await _database.Purchases
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<PurchaseHistory?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _database.Purchases
                .AsNoTracking()
                .FirstOrDefaultAsync(purchaseHistory => purchaseHistory.Id == id, ct);
        }

        public async Task AddAsync(PurchaseHistory purchaseHistory, CancellationToken ct = default)
        {
            await _database.Purchases.AddAsync(purchaseHistory, ct);
        }

        public Task UpdateAsync(PurchaseHistory purchaseHistory, CancellationToken ct = default)
        {
            _database.Purchases.Update(purchaseHistory);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _database.Purchases.FindAsync(new object[] { id }, ct);
            if (entity is not null)
            {
                _database.Purchases.Remove(entity);
            }
        }
    }
}
