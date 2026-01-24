using ImagesShop.API.Data;
using ImagesShop.API.Models;
using ImagesShop.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.API.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _database;

        public CategoryRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default)
        {
            return await _database.Categories
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _database.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == id, ct);
        }

        public async Task AddAsync(Category category, CancellationToken ct = default)
        {
            await _database.Categories.AddAsync(category, ct);
        }

        public Task UpdateAsync(Category category, CancellationToken ct = default)
        {
            _database.Categories.Update(category);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _database.Categories.FindAsync(new object[] { id }, ct);
            if (entity is not null)
            {
                _database.Categories.Remove(entity);
            }
        }
    }
}
