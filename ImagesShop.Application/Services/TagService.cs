using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _repository;

        public TagService(ITagRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }

        public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) return null;
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Tag> CreateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            if (tag is null) throw new ArgumentNullException(nameof(tag));

            if (tag.Id == Guid.Empty)
                tag.Id = Guid.NewGuid();

            tag.Name ??= string.Empty;

            await _repository.AddAsync(tag, cancellationToken);
            return tag;
        }

        public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            if (tag is null) throw new ArgumentNullException(nameof(tag));
            if (tag.Id == Guid.Empty) throw new ArgumentException("Tag must have an Id", nameof(tag));

            var existing = await _repository.GetByIdAsync(tag.Id, cancellationToken);
            if (existing is null) throw new InvalidOperationException("Tag not found");

            existing.Name = tag.Name ?? existing.Name;

            await _repository.UpdateAsync(existing, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid id", nameof(id));

            await _repository.DeleteAsync(id, cancellationToken);
        }
    }
}