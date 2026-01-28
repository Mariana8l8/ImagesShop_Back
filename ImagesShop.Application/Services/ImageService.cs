using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class ImageService : IImageService
    { 
        private readonly IImageRepository _repository;

        public ImageService(IImageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }

        public async Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) return null;
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Image> CreateAsync(Image image, CancellationToken cancellationToken = default)
        {
            if (image is null) throw new ArgumentNullException(nameof(image));

            if (image.Id == Guid.Empty)
                image.Id = Guid.NewGuid();

            image.Price = image.Price < 0m ? 0m : image.Price;
            image.Title ??= string.Empty;
            image.Description ??= string.Empty;
            image.WatermarkedUrl ??= string.Empty;
            image.OriginalUrl ??= string.Empty;

            await _repository.AddAsync(image, cancellationToken);
            return image;
        }

        public async Task UpdateAsync(Image image, CancellationToken cancellationToken = default)
        {
            if (image is null) throw new ArgumentNullException(nameof(image));
            if (image.Id == Guid.Empty) throw new ArgumentException("Image must have an Id", nameof(image));

            var existing = await _repository.GetByIdAsync(image.Id, cancellationToken);
            if (existing is null) throw new InvalidOperationException("Image not found");

            existing.Title = image.Title;
            existing.Description = image.Description;
            existing.Price = image.Price < 0m ? 0m : image.Price;
            existing.WatermarkedUrl = image.WatermarkedUrl;
            existing.OriginalUrl = image.OriginalUrl;
            existing.CategoryId = image.CategoryId;

            if (image.Tags is not null && image.Tags.Count > 0)
            {
                existing.Tags.Clear();
                foreach (var tag in image.Tags)
                {
                    existing.Tags.Add(new ImageTag { ImageId = existing.Id, TagId = tag.TagId });
                }
            }

            await _repository.UpdateAsync(existing, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid id", nameof(id));

            await _repository.DeleteAsync(id, cancellationToken);
        }
    }
}