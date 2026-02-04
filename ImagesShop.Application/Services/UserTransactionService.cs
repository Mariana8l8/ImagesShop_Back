using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class UserTransactionService : IUserTransactionService
    {
        private readonly IUserTransactionRepository _transactions;

        public UserTransactionService(IUserTransactionRepository transactions)
        {
            _transactions = transactions;
        }

        public Task<IEnumerable<UserTransaction>> GetMyAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) return Task.FromResult<IEnumerable<UserTransaction>>(Array.Empty<UserTransaction>());
            return _transactions.GetByUserIdAsync(userId, cancellationToken);
        }
    }
}
