using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _categoryRepository.GetAllAsync(cancellationToken);
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) 
            {
                return null;
            }
            
            return await _categoryRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
        {
            if (category is null) throw new ArgumentNullException(nameof(category));

            if (category.Id == Guid.Empty)
            {
                category.Id = Guid.NewGuid();
            }

            category.Name ??= string.Empty;

            await _categoryRepository.AddAsync(category, cancellationToken);
            
            return category;
        }

        public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            if (category is null) throw new ArgumentNullException(nameof(category));
            if (category.Id == Guid.Empty) throw new ArgumentException("The category instance must have a valid identifier.", nameof(category));

            var existingCategory = await _categoryRepository.GetByIdAsync(category.Id, cancellationToken);
            if (existingCategory is null) 
            {
                throw new InvalidOperationException("The category to update was not found.");
            }

            existingCategory.Name = category.Name ?? existingCategory.Name;

            await _categoryRepository.UpdateAsync(existingCategory, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("The category identifier cannot be empty.", nameof(id));

            await _categoryRepository.DeleteAsync(id, cancellationToken);
        }
    }
}