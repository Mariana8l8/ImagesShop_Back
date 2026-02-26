using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using ImagesShop.Domain.Enums;

namespace ImagesShop.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IImageRepository _imageRepository;
        private readonly IPurchaseHistoryRepository _purchaseHistoryRepository;
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IImageRepository imageRepository,
            IPurchaseHistoryRepository purchaseHistoryRepository,
            IOrderItemRepository orderItemRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _imageRepository = imageRepository;
            _purchaseHistoryRepository = purchaseHistoryRepository;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _orderRepository.GetAllAsync(cancellationToken);
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) 
            {
                return null;
            }
            
            return await _orderRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order is null) throw new ArgumentNullException(nameof(order));
            
            if (order.Id == Guid.Empty) 
            {
                order.Id = Guid.NewGuid();
            }
            
            await _orderRepository.AddAsync(order, cancellationToken);
            
            return order;
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order is null) throw new ArgumentNullException(nameof(order));
            
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("The order identifier cannot be empty.", nameof(id));
            
            await _orderRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<PurchaseResultDTO> PurchaseImageAsync(Guid userId, Guid imageId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("User identifier cannot be empty.", nameof(userId));
            if (imageId == Guid.Empty) throw new ArgumentException("Image identifier cannot be empty.", nameof(imageId));

            var userEntity = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (userEntity is null) 
            {
                throw new InvalidOperationException("The user was not found.");
            }

            var imageEntity = await _imageRepository.GetByIdAsync(imageId, cancellationToken);
            if (imageEntity is null) 
            {
                throw new KeyNotFoundException("The requested image was not found.");
            }

            if (userEntity.Balance < imageEntity.Price) 
            {
                throw new InvalidOperationException("Insufficient balance to complete the purchase.");
            }

            var newOrder = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userEntity.Id,
                Status = OrderStatus.Completed,
                TotalAmount = imageEntity.Price,
                Currency = "USD",
                Items = new List<OrderItem>()
            };

            await _orderRepository.AddAsync(newOrder, cancellationToken);

            var newOrderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = newOrder.Id,
                ImageId = imageEntity.Id
            };
            
            await _orderItemRepository.AddAsync(newOrderItem, cancellationToken);

            userEntity.Balance -= imageEntity.Price;
            await _userRepository.UpdateAsync(userEntity, cancellationToken);

            var purchaseHistoryRecord = new PurchaseHistory
            {
                Id = Guid.NewGuid(),
                UserName = userEntity.Name,
                UserEmail = userEntity.Email,
                ImageId = imageEntity.Id,
                ImagePrice = imageEntity.Price,
                ImageTitle = imageEntity.Title,
                PurchasedAt = DateTime.UtcNow
            };
            
            await _purchaseHistoryRepository.AddAsync(purchaseHistoryRecord, cancellationToken);

            return new PurchaseResultDTO
            {
                OrderId = newOrder.Id,
                OriginalUrl = imageEntity.OriginalUrl
            };
        }
    }
}