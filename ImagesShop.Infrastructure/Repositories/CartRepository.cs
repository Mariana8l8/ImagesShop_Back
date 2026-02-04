using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _db;

        public CartRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CartItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.CartItems
                .AsNoTracking()
                .Include(ci => ci.Image)
                .Where(ci => ci.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public Task<CartItem?> GetItemAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default)
        {
            return _db.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ImageId == imageId, cancellationToken);
        }

        public async Task AddAsync(CartItem item, CancellationToken cancellationToken = default)
        {
            await _db.CartItems.AddAsync(item, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveAsync(CartItem item, CancellationToken cancellationToken = default)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await _db.CartItems.Where(ci => ci.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
