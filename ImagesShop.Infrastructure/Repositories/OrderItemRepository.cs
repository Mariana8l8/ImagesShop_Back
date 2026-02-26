using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _appDbContext;

        public OrderItemRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _appDbContext.OrderItems
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<OrderItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.OrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(orderItem => orderItem.Id == id, cancellationToken);
        }

        public async Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            await _appDbContext.OrderItems.AddAsync(orderItem, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            _appDbContext.OrderItems.Update(orderItem);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var orderItemEntity = await _appDbContext.OrderItems.FindAsync(new object[] { id }, cancellationToken);
            
            if (orderItemEntity is not null)
            {
                _appDbContext.OrderItems.Remove(orderItemEntity);
                await _appDbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.OrderItems
                .AsNoTracking()
                .Where(orderItem => orderItem.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }
    }
}
