using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemsRepository;

        public OrderItemService(IOrderItemRepository orderItems)
        {
            _orderItemsRepository = orderItems;
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _orderItemsRepository.GetAllAsync(cancellationToken);
        }

        public async Task<OrderItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _orderItemsRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _orderItemsRepository.GetByOrderIdAsync(orderId, cancellationToken);
        }

        public async Task<OrderItem> CreateAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            if (orderItem.Id == Guid.Empty) orderItem.Id = Guid.NewGuid();
            await _orderItemsRepository.AddAsync(orderItem, cancellationToken);
            return orderItem;
        }

        public async Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            _ = orderItem ?? throw new ArgumentNullException(nameof(orderItem));
            await _orderItemsRepository.UpdateAsync(orderItem, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _orderItemsRepository.DeleteAsync(id, cancellationToken);
        }
    }
}