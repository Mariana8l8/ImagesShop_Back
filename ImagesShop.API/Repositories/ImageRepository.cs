using ImagesShop.API.Data;
using ImagesShop.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.API.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _database;

        public ImageRepository(AppDbContext database)
        {
            _database = database;
        }
        public async Task<IEnumerable<Image>> GetAllAsync(CancellationToken ct = default)
        {
            return await _database.Images
                .AsNoTracking()
                .ToListAsync(ct);
        }
        public async Task<Image?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _database.Images
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(image => image.Id == id, ct);
        }

        public async Task AddAsync(Image image, CancellationToken ct = default)
        {
            await _database.Images.AddAsync(image, ct);
        }

        public Task UpdateAsync(Image image, CancellationToken ct = default)
        {
            _database.Images.Update(image);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _database.Images.FindAsync(new object[] { id }, ct);
            if (entity is not null)
            {
                _database.Images.Remove(entity);
            }
        }
    }
}