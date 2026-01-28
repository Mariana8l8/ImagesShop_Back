using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IServices
{
    public interface IImageService
    {
        Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Image> CreateAsync(Image image, CancellationToken cancellationToken = default);
        Task UpdateAsync(Image image, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}