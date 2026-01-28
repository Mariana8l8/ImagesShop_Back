using ImagesShop.Domain.Entities;
 
namespace ImagesShop.Application.Interfaces.IRepositories
{
    public interface IImageRepository
    {
        Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Image image, CancellationToken cancellationToken = default);
        Task UpdateAsync(Image image, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}