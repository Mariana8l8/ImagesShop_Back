using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Domain.Entities;
using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _database;

        public UserRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _database.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _database.Users
                .Include(user => user.RefreshTokens)
                .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _database.Users
                .Include(user => user.RefreshTokens)
                .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _database.Users.AddAsync(user, cancellationToken);
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            await _database.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _database.Users.FindAsync(new object[] { id }, cancellationToken);
            if (entity is not null)
            {
                _database.Users.Remove(entity);
                await _database.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
