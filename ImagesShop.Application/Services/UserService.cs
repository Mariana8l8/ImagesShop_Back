using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using ImagesShop.Domain.Enums;

namespace ImagesShop.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) return null;
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _repository.GetByEmailAsync(email, cancellationToken);
        }

        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            if (user.Id == Guid.Empty)
                user.Id = Guid.NewGuid();

            user.Email ??= string.Empty;
            user.Name ??= string.Empty;
            user.PasswordHash ??= string.Empty;
            user.PasswordSalt ??= string.Empty;
            user.Balance = user.Balance < 0m ? 0m : user.Balance;
            user.Role = user.Role == 0 ? UserRole.User : user.Role;
            user.Wishlist ??= new List<Image>();
            user.Orders ??= new List<Order>();
            user.RefreshTokens ??= new List<RefreshToken>();

            await _repository.AddAsync(user, cancellationToken);
            return user;
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (user.Id == Guid.Empty) throw new ArgumentException("User must have an Id", nameof(user));

            var existing = await _repository.GetByIdAsync(user.Id, cancellationToken);
            if (existing is null) throw new InvalidOperationException("User not found");

            existing.Name = user.Name ?? existing.Name;
            existing.Email = user.Email ?? existing.Email;
            existing.PasswordHash = string.IsNullOrWhiteSpace(user.PasswordHash) ? existing.PasswordHash : user.PasswordHash;
            existing.PasswordSalt = string.IsNullOrWhiteSpace(user.PasswordSalt) ? existing.PasswordSalt : user.PasswordSalt;
            existing.Balance = user.Balance;
            existing.Role = user.Role;

            await _repository.UpdateAsync(existing, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid id", nameof(id));

            await _repository.DeleteAsync(id, cancellationToken);
        }

        public async Task UpdateNameAsync(Guid userId, string name, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid id", nameof(userId));
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Name is required");

            var existing = await _repository.GetByIdAsync(userId, cancellationToken);
            if (existing is null) throw new InvalidOperationException("User not found");

            existing.Name = name.Trim();
            await _repository.UpdateAsync(existing, cancellationToken);
        }
    }
}