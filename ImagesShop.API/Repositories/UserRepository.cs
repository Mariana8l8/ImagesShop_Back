using ImagesShop.API.Data;
using ImagesShop.API.Models;
using ImagesShop.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _database;

        public UserRepository(AppDbContext database)
        {
            _database = database;
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        {
            return await _database.Users
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _database.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == id, ct);
        }

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _database.Users.AddAsync(user, ct);
        }

        public Task UpdateAsync(User user, CancellationToken ct = default)
        {
            _database.Users.Update(user);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _database.Users.FindAsync(new object[] { id }, ct);
            if (entity is not null) 
            {
                _database.Users.Remove(entity);
            }
        }
    }
}
