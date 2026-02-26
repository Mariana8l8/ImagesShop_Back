using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public Task<IEnumerable<CartItem>> GetMyCartAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) 
            {
                return Task.FromResult<IEnumerable<CartItem>>(Array.Empty<CartItem>());
            }
            
            return _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        }

        public async Task AddAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("User identifier cannot be empty.", nameof(userId));
            if (imageId == Guid.Empty) throw new ArgumentException("Image identifier cannot be empty.", nameof(imageId));

            var existingCartItem = await _cartRepository.GetItemAsync(userId, imageId, cancellationToken);
            if (existingCartItem is not null) 
            {
                return;
            }

            var newCartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ImageId = imageId
            };

            await _cartRepository.AddAsync(newCartItem, cancellationToken);
        }

        public async Task RemoveAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("User identifier cannot be empty.", nameof(userId));
            if (imageId == Guid.Empty) throw new ArgumentException("Image identifier cannot be empty.", nameof(imageId));

            var existingCartItem = await _cartRepository.GetItemAsync(userId, imageId, cancellationToken);
            if (existingCartItem is null) 
            {
                return;
            }

            await _cartRepository.RemoveAsync(existingCartItem, cancellationToken);
        }

        public Task ClearAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) 
            {
                return Task.CompletedTask;
            }
            
            return _cartRepository.ClearAsync(userId, cancellationToken);
        }
    }
}
