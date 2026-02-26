using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _appDbContext;

        public TagRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Tags
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(tag => tag.Id == id, cancellationToken);
        }

        public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Tags.AddAsync(tag, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            _appDbContext.Tags.Update(tag);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var tagEntity = await _appDbContext.Tags.FindAsync(new object[] { id }, cancellationToken);
            
            if (tagEntity is not null)
            {
                _appDbContext.Tags.Remove(tagEntity);
                await _appDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
