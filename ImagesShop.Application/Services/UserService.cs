using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

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

        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            if (user.Id == Guid.Empty)
                user.Id = Guid.NewGuid();

            user.Email ??= string.Empty;
            user.Name ??= string.Empty;
            user.PasswordHash ??= string.Empty;
            user.Balance = user.Balance < 0m ? 0m : user.Balance;
            user.Wishlist ??= new List<Image>();
            user.Orders ??= new List<Order>();

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
            existing.PasswordHash = user.PasswordHash ?? existing.PasswordHash;

            existing.Balance = user.Balance;

            await _repository.UpdateAsync(existing, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid id", nameof(id));

            await _repository.DeleteAsync(id, cancellationToken);
        }
    }
}