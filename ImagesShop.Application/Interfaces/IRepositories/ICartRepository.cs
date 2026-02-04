using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IRepositories
{
    public interface ICartRepository
    {
        Task<IEnumerable<CartItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<CartItem?> GetItemAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default);
        Task AddAsync(CartItem item, CancellationToken cancellationToken = default);
        Task RemoveAsync(CartItem item, CancellationToken cancellationToken = default);
        Task ClearAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
