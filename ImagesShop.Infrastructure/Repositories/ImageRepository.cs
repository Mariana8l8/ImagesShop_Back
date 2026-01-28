using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _database;

        public ImageRepository(AppDbContext database)
        {
            _database = database;
        }
        public async Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _database.Images
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        public async Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _database.Images
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(image => image.Id == id, cancellationToken);
        }

        public async Task AddAsync(Image image, CancellationToken cancellationToken = default)
        {
            await _database.Images.AddAsync(image, cancellationToken);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Image image, CancellationToken cancellationToken = default)
        {
            _database.Images.Update(image);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _database.Images.FindAsync(new object[] { id }, cancellationToken);
            if (entity is not null)
            {
                _database.Images.Remove(entity);
                await _database.SaveChangesAsync(cancellationToken);
            }
        }
    }
}