using ImagesShop.API.Data;
using ImagesShop.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.API.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _db;

        public ImageRepository(AppDbContext db)
        {
            _db = db; 
        }

        public async Task AddAsync(Image image, CancellationToken ct = default)
        {
            await _db.Images.AddAsync(image, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Images.FindAsync(new object[] { id }, ct);
            if (entity is not null)
                _db.Images.Remove(entity);
        }

        public async Task<IEnumerable<Image>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Images
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Image?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Images
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(i => i.Id == id, ct);
        }

        public Task UpdateAsync(Image image, CancellationToken ct = default)
        {
            _db.Images.Update(image);
            return Task.CompletedTask;
        }
    }
}