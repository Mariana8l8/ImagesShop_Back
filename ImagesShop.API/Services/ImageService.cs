using ImagesShop.API.Data;
using ImagesShop.API.Models;
using ImagesShop.API.Repositories;
using ImagesShop.API.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImagesShop.API.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _repo;
        private readonly AppDbContext _db;

        public ImageService(IImageRepository repo, AppDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public async Task<IEnumerable<Image>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.GetAllAsync(ct);
        }

        public async Task<Image?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty) return null;
            return await _repo.GetByIdAsync(id, ct);
        }

        public async Task<Image> CreateAsync(Image image, CancellationToken ct = default)
        {
            if (image is null) throw new ArgumentNullException(nameof(image));

            if (image.Id == Guid.Empty)
                image.Id = Guid.NewGuid();

            image.Price = image.Price < 0m ? 0m : image.Price;
            image.Title ??= string.Empty;
            image.Description ??= string.Empty;
            image.WatermarkedUrl ??= string.Empty;
            image.OriginalUrl ??= string.Empty;

            await _repo.AddAsync(image, ct);
            await _db.SaveChangesAsync(ct);
            return image;
        }

        public async Task UpdateAsync(Image image, CancellationToken ct = default)
        {
            if (image is null) throw new ArgumentNullException(nameof(image));
            if (image.Id == Guid.Empty) throw new ArgumentException("Image must have an Id", nameof(image));

            var existing = await _repo.GetByIdAsync(image.Id, ct);
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
                foreach (var t in image.Tags)
                {
                    existing.Tags.Add(new ImageTag { ImageId = existing.Id, TagId = t.TagId });
                }
            }

            await _repo.UpdateAsync(existing, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid id", nameof(id));

            await _repo.DeleteAsync(id, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}