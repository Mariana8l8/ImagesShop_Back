using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class PurchaseHistoryRepository : IPurchaseHistoryRepository
    {
        private readonly AppDbContext _appDbContext;

        public PurchaseHistoryRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<PurchaseHistory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Purchases
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<PurchaseHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Purchases
                .AsNoTracking()
                .FirstOrDefaultAsync(purchaseHistory => purchaseHistory.Id == id, cancellationToken);
        }

        public async Task AddAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Purchases.AddAsync(purchaseHistory, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default)
        {
            _appDbContext.Purchases.Update(purchaseHistory);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var purchaseHistoryEntity = await _appDbContext.Purchases.FindAsync(new object[] { id }, cancellationToken);
            
            if (purchaseHistoryEntity is not null)
            {
                _appDbContext.Purchases.Remove(purchaseHistoryEntity);
                await _appDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
