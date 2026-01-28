using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _database;

        public TagRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _database.Tags
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _database.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
        }

        public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            await _database.Tags.AddAsync(tag, cancellationToken);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            _database.Tags.Update(tag);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _database.Tags.FindAsync(new object[] { id }, cancellationToken);
            if (entity is not null)
            {
                _database.Tags.Remove(entity);
                await _database.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
