using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _appDbContext;

        public CartRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<CartItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.CartItems
                .AsNoTracking()
                .Include(cartItem => cartItem.Image)
                .Where(cartItem => cartItem.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public Task<CartItem?> GetItemAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default)
        {
            return _appDbContext.CartItems.FirstOrDefaultAsync(cartItem => cartItem.UserId == userId && cartItem.ImageId == imageId, cancellationToken);
        }

        public async Task AddAsync(CartItem cartItem, CancellationToken cancellationToken = default)
        {
            await _appDbContext.CartItems.AddAsync(cartItem, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveAsync(CartItem cartItem, CancellationToken cancellationToken = default)
        {
            _appDbContext.CartItems.Remove(cartItem);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await _appDbContext.CartItems
                .Where(cartItem => cartItem.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
