using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _database;

        public CategoryRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _database.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _database.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        }

        public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _database.Categories.AddAsync(category, cancellationToken);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            _database.Categories.Update(category);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _database.Categories.FindAsync(new object[] { id }, cancellationToken);
            if (entity is not null)
            {
                _database.Categories.Remove(entity);
                await _database.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
