using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _database;

        public OrderItemRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _database.OrderItems
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<OrderItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _database.OrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(oi => oi.Id == id, cancellationToken);
        }

        public async Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            await _database.OrderItems.AddAsync(orderItem, cancellationToken);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            _database.OrderItems.Update(orderItem);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _database.OrderItems.FindAsync(new object[] { id }, cancellationToken);
            if (entity is not null)
            {
                _database.OrderItems.Remove(entity);
                await _database.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _database.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }
    }
}
