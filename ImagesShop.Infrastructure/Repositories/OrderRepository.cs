using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _appDbContext;

        public OrderRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Orders
                .AsNoTracking()
                .Include(order => order.Items)
                .ToListAsync(cancellationToken);
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Orders
                .AsNoTracking()
                .Include(order => order.Items)
                .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Orders.AddAsync(order, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            _appDbContext.Orders.Update(order);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var orderEntity = await _appDbContext.Orders.FindAsync(new object[] { id }, cancellationToken);
            
            if (orderEntity is not null)
            {
                _appDbContext.Orders.Remove(orderEntity);
                await _appDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}