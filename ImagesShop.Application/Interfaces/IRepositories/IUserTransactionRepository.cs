using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IRepositories
{
    public interface IUserTransactionRepository
    {
        Task<IEnumerable<UserTransaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(UserTransaction transaction, CancellationToken cancellationToken = default);
    }
}
