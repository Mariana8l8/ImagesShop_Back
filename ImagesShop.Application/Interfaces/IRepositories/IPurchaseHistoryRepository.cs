using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IRepositories
{
    public interface IPurchaseHistoryRepository
    {
        Task<IEnumerable<PurchaseHistory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PurchaseHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default);
        Task UpdateAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
