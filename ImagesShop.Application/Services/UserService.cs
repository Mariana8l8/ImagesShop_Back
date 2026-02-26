using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using ImagesShop.Domain.Enums;

namespace ImagesShop.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserTransactionRepository _userTransactionRepository;

        public UserService(IUserRepository userRepository, IUserTransactionRepository userTransactionRepository)
        {
            _userRepository = userRepository;
            _userTransactionRepository = userTransactionRepository;
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetAllAsync(cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) 
            {
                return null;
            }
            
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email)) 
            {
                return null;
            }
            
            return await _userRepository.GetByEmailAsync(email, cancellationToken);
        }

        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            user.Email ??= string.Empty;
            user.Name ??= string.Empty;
            user.PasswordHash ??= string.Empty;
            user.PasswordSalt ??= string.Empty;
            user.Balance = user.Balance < 0m ? 0m : user.Balance;
            user.Role = user.Role == 0 ? UserRole.User : user.Role;
            user.Wishlist ??= new List<Image>();
            user.Orders ??= new List<Order>();
            user.RefreshTokens ??= new List<RefreshToken>();

            await _userRepository.AddAsync(user, cancellationToken);
            
            return user;
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (user.Id == Guid.Empty) throw new ArgumentException("The user must have a valid identifier.", nameof(user));

            var existingUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);
            if (existingUser is null) 
            {
                throw new InvalidOperationException("The user to update was not found.");
            }

            existingUser.Name = user.Name ?? existingUser.Name;
            existingUser.Email = user.Email ?? existingUser.Email;
            existingUser.PasswordHash = string.IsNullOrWhiteSpace(user.PasswordHash) ? existingUser.PasswordHash : user.PasswordHash;
            existingUser.PasswordSalt = string.IsNullOrWhiteSpace(user.PasswordSalt) ? existingUser.PasswordSalt : user.PasswordSalt;
            existingUser.Balance = user.Balance;
            existingUser.Role = user.Role;

            await _userRepository.UpdateAsync(existingUser, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("The user identifier cannot be empty.", nameof(id));

            await _userRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task UpdateNameAsync(Guid userId, string name, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid user identifier.", nameof(userId));
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("The user name is required.");

            var existingUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (existingUser is null) 
            {
                throw new InvalidOperationException("The user was not found.");
            }

            existingUser.Name = name.Trim();
            await _userRepository.UpdateAsync(existingUser, cancellationToken);
        }

        public async Task<decimal> TopUpAsync(Guid userId, decimal amount, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid user identifier.", nameof(userId));
            if (amount <= 0) throw new InvalidOperationException("The top-up amount must be positive.");

            var userEntity = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (userEntity is null) 
            {
                throw new InvalidOperationException("The user was not found.");
            }

            var balanceBeforeUpdate = userEntity.Balance;
            userEntity.Balance += amount;
            
            await _userRepository.UpdateAsync(userEntity, cancellationToken);

            var topUpTransaction = new UserTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userEntity.Id,
                Type = UserTransactionType.TopUp,
                Amount = amount,
                BalanceBefore = balanceBeforeUpdate,
                BalanceAfter = userEntity.Balance,
                CreatedAt = DateTime.UtcNow,
                OrderId = null,
                Status = UserTransactionStatus.Success
            };

            await _userTransactionRepository.AddAsync(topUpTransaction, cancellationToken);

            return userEntity.Balance;
        }
    }
}