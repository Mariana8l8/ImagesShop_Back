using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Services
{
    public class ImageService : IImageService
    { 
        private readonly IImageRepository _imageRepository;

        public ImageService(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public async Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _imageRepository.GetAllAsync(cancellationToken);
        }

        public async Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) 
            {
                return null;
            }
            
            return await _imageRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Image> CreateAsync(Image image, CancellationToken cancellationToken = default)
        {
            if (image is null) throw new ArgumentNullException(nameof(image));

            if (image.Id == Guid.Empty)
            {
                image.Id = Guid.NewGuid();
            }

            image.Price = image.Price < 0m ? 0m : image.Price;
            image.Title ??= string.Empty;
            image.Description ??= string.Empty;
            image.WatermarkedUrl ??= string.Empty;
            image.OriginalUrl ??= string.Empty;

            await _imageRepository.AddAsync(image, cancellationToken);
            
            return image;
        }

        public async Task UpdateAsync(Image image, CancellationToken cancellationToken = default)
        {
            if (image is null) throw new ArgumentNullException(nameof(image));
            if (image.Id == Guid.Empty) throw new ArgumentException("The image instance must have a valid identifier.", nameof(image));

            var existingImage = await _imageRepository.GetByIdAsync(image.Id, cancellationToken);
            if (existingImage is null) 
            {
                throw new InvalidOperationException("The image to update was not found.");
            }

            existingImage.Title = image.Title;
            existingImage.Description = image.Description;
            existingImage.Price = image.Price < 0m ? 0m : image.Price;
            existingImage.WatermarkedUrl = image.WatermarkedUrl;
            existingImage.OriginalUrl = image.OriginalUrl;
            existingImage.CategoryId = image.CategoryId;

            if (image.Tags is not null && image.Tags.Count > 0)
            {
                existingImage.Tags.Clear();
                foreach (var imageTag in image.Tags)
                {
                    existingImage.Tags.Add(new ImageTag { ImageId = existingImage.Id, TagId = imageTag.TagId });
                }
            }

            await _imageRepository.UpdateAsync(existingImage, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("The image identifier cannot be empty.", nameof(id));

            await _imageRepository.DeleteAsync(id, cancellationToken);
        }
    }
}