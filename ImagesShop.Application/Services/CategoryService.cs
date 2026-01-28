using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) return null;
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
        {
            if (category is null) throw new ArgumentNullException(nameof(category));

            if (category.Id == Guid.Empty)
                category.Id = Guid.NewGuid();

            category.Name ??= string.Empty;

            await _repository.AddAsync(category, cancellationToken);
            return category;
        }

        public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            if (category is null) throw new ArgumentNullException(nameof(category));
            if (category.Id == Guid.Empty) throw new ArgumentException("Category must have an Id", nameof(category));

            var existing = await _repository.GetByIdAsync(category.Id, cancellationToken);
            if (existing is null) throw new InvalidOperationException("Category not found");

            existing.Name = category.Name ?? existing.Name;

            await _repository.UpdateAsync(existing, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid id", nameof(id));

            await _repository.DeleteAsync(id, cancellationToken);
        }
    }
}