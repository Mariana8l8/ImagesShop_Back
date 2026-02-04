using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IServices
{
    public interface IUserTransactionService
    {
        Task<IEnumerable<UserTransaction>> GetMyAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
