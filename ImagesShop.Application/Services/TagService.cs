using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _tagRepository.GetAllAsync(cancellationToken);
        }

        public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) 
            {
                return null;
            }
            
            return await _tagRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Tag> CreateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            if (tag is null) throw new ArgumentNullException(nameof(tag));

            if (tag.Id == Guid.Empty)
            {
                tag.Id = Guid.NewGuid();
            }

            tag.Name ??= string.Empty;

            await _tagRepository.AddAsync(tag, cancellationToken);
            
            return tag;
        }

        public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            if (tag is null) throw new ArgumentNullException(nameof(tag));
            if (tag.Id == Guid.Empty) throw new ArgumentException("The tag instance must have a valid identifier.", nameof(tag));

            var existingTag = await _tagRepository.GetByIdAsync(tag.Id, cancellationToken);
            if (existingTag is null) 
            {
                throw new InvalidOperationException("The tag to update was not found.");
            }

            existingTag.Name = tag.Name ?? existingTag.Name;

            await _tagRepository.UpdateAsync(existingTag, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("The tag identifier cannot be empty.", nameof(id));

            await _tagRepository.DeleteAsync(id, cancellationToken);
        }
    }
}