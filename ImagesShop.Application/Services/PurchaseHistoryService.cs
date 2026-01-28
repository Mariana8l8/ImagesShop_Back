using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class PurchaseHistoryService : IPurchaseHistoryService
    {
        private readonly IPurchaseHistoryRepository _repository;

        public PurchaseHistoryService(IPurchaseHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PurchaseHistory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }

        public async Task<PurchaseHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) return null;
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<PurchaseHistory> CreateAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default)
        {
            if (purchaseHistory is null) throw new ArgumentNullException(nameof(purchaseHistory));

            if (purchaseHistory.Id == Guid.Empty)
                purchaseHistory.Id = Guid.NewGuid();

            purchaseHistory.UserName ??= string.Empty;
            purchaseHistory.UserEmail ??= string.Empty;
            purchaseHistory.ImageTitle ??= string.Empty;
            purchaseHistory.ImagePrice = purchaseHistory.ImagePrice < 0m ? 0m : purchaseHistory.ImagePrice;
            purchaseHistory.PurchasedAt = purchaseHistory.PurchasedAt == default ? DateTime.UtcNow : purchaseHistory.PurchasedAt;

            await _repository.AddAsync(purchaseHistory, cancellationToken);
            return purchaseHistory;
        }

        public async Task UpdateAsync(PurchaseHistory purchaseHistory, CancellationToken cancellationToken = default)
        {
            if (purchaseHistory is null) throw new ArgumentNullException(nameof(purchaseHistory));
            if (purchaseHistory.Id == Guid.Empty) throw new ArgumentException("PurchaseHistory must have an Id", nameof(purchaseHistory));

            var existing = await _repository.GetByIdAsync(purchaseHistory.Id, cancellationToken);
            if (existing is null) throw new InvalidOperationException("PurchaseHistory not found");

            existing.UserName = purchaseHistory.UserName ?? existing.UserName;
            existing.UserEmail = purchaseHistory.UserEmail ?? existing.UserEmail;
            existing.ImageTitle = purchaseHistory.ImageTitle ?? existing.ImageTitle;
            existing.ImagePrice = purchaseHistory.ImagePrice;

            await _repository.UpdateAsync(existing, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid id", nameof(id));

            await _repository.DeleteAsync(id, cancellationToken);
        }
    }
}