using ImagesShop.API.Models;
 
namespace ImagesShop.API.Repositories
{
    public interface IImageRepository
    {
        Task<IEnumerable<Image>> GetAllAsync(CancellationToken ct = default);
        Task<Image?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Image image, CancellationToken ct = default);
        Task UpdateAsync(Image image, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}