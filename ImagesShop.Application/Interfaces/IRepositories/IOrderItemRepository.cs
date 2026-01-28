using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IRepositories
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<OrderItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
        Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}