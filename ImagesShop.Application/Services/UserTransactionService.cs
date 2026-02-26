using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class UserTransactionService : IUserTransactionService
    {
        private readonly IUserTransactionRepository _userTransactionRepository;

        public UserTransactionService(IUserTransactionRepository userTransactionRepository)
        {
            _userTransactionRepository = userTransactionRepository;
        }

        public Task<IEnumerable<UserTransaction>> GetMyAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) 
            {
                return Task.FromResult<IEnumerable<UserTransaction>>(Array.Empty<UserTransaction>());
            }
            
            return _userTransactionRepository.GetByUserIdAsync(userId, cancellationToken);
        }
    }
}
