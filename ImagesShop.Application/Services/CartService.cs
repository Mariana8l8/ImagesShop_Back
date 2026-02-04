using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cart;

        public CartService(ICartRepository cart)
        {
            _cart = cart;
        }

        public Task<IEnumerable<CartItem>> GetMyCartAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) return Task.FromResult<IEnumerable<CartItem>>(Array.Empty<CartItem>());
            return _cart.GetByUserIdAsync(userId, cancellationToken);
        }

        public async Task AddAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid userId", nameof(userId));
            if (imageId == Guid.Empty) throw new ArgumentException("Invalid imageId", nameof(imageId));

            var existing = await _cart.GetItemAsync(userId, imageId, cancellationToken);
            if (existing is not null) return;

            await _cart.AddAsync(new CartItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ImageId = imageId
            }, cancellationToken);
        }

        public async Task RemoveAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid userId", nameof(userId));
            if (imageId == Guid.Empty) throw new ArgumentException("Invalid imageId", nameof(imageId));

            var existing = await _cart.GetItemAsync(userId, imageId, cancellationToken);
            if (existing is null) return;

            await _cart.RemoveAsync(existing, cancellationToken);
        }

        public Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) return Task.CompletedTask;
            return _cart.ClearAsync(userId, cancellationToken);
        }
    }
}
