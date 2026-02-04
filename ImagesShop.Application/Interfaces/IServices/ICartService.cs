using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IServices
{
    public interface ICartService
    {
        Task<IEnumerable<CartItem>> GetMyCartAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default);
        Task RemoveAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default);
        Task ClearAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
