using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _database;

        public OrderRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _database.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .ToListAsync(cancellationToken);
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _database.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _database.Orders.AddAsync(order, cancellationToken);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            _database.Orders.Update(order);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _database.Orders.FindAsync(new object[] { id }, cancellationToken);
            if (entity is not null)
            {
                _database.Orders.Remove(entity);
                await _database.SaveChangesAsync(cancellationToken);
            }
        }
    }
}