using ImagesShop.API.Models;

namespace ImagesShop.API.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default);
        Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Category category, CancellationToken ct = default);
        Task UpdateAsync(Category category, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
