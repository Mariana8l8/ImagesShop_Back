using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IRepositories
{
    public interface ITagRepository
    {
        Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
        Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
