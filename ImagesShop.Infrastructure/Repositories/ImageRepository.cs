using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _appDbContext;

        public ImageRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Images
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Images
                .Include(image => image.Tags)
                .FirstOrDefaultAsync(image => image.Id == id, cancellationToken);
        }

        public async Task AddAsync(Image image, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Images.AddAsync(image, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Image image, CancellationToken cancellationToken = default)
        {
            _appDbContext.Images.Update(image);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var imageEntity = await _appDbContext.Images.FindAsync(new object[] { id }, cancellationToken);
            
            if (imageEntity is not null)
            {
                _appDbContext.Images.Remove(imageEntity);
                await _appDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}