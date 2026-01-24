using ImagesShop.API.Models;

namespace ImagesShop.API.Repositories.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<PurchaseHistory>> GetAllAsync(CancellationToken ct = default);
        Task<PurchaseHistory?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(PurchaseHistory purchaseHistory, CancellationToken ct = default);
        Task UpdateAsync(PurchaseHistory purchaseHistory, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
