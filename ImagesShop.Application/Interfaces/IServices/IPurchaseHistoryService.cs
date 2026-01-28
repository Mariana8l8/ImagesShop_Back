using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IServices
{
    public interface IPurchaseHistoryService
    {
        Task<IEnumerable<PurchaseHistory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PurchaseHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PurchaseHistory> CreateAsync(PurchaseHistory PurchaseHistory, CancellationToken cancellationToken = default);
        Task UpdateAsync(PurchaseHistory PurchaseHistory, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
