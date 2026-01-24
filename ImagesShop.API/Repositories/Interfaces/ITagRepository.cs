using ImagesShop.API.Models;

namespace ImagesShop.API.Repositories.Interfaces
{
    public interface ITagRepository
    {
        Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct = default);
        Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Tag tag, CancellationToken ct = default);
        Task UpdateAsync(Tag tag, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
