using ImagesShop.API.Data;
using ImagesShop.API.Models;
using ImagesShop.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.API.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _database;

        public TagRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct = default)
        {
            return await _database.Tags
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _database.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == id, ct);
        }

        public async Task AddAsync(Tag tag, CancellationToken ct = default)
        {
            await _database.Tags.AddAsync(tag, ct);
        }

        public Task UpdateAsync(Tag tag, CancellationToken ct = default)
        {
            _database.Tags.Update(tag);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _database.Tags.FindAsync(new object[] { id }, ct);
            if (entity is not null)
            {
                _database.Tags.Remove(entity);
            }
        }
    }
}
