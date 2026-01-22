using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ImagesShop.API.Models;

namespace ImagesShop.API.Services.Interfaces
{
    public interface IImageService 
    {
        Task<IEnumerable<Image>> GetAllAsync(CancellationToken ct = default);
        Task<Image?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Image> CreateAsync(Image image, CancellationToken ct = default);
        Task UpdateAsync(Image image, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}