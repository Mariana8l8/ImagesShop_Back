using ImagesShop.Domain.Entities;

namespace ImagesShop.Application.Interfaces.IRepositories
{
    public interface IPendingRegistrationRepository
    {
        Task<PendingRegistration?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task UpsertAsync(PendingRegistration pending, CancellationToken cancellationToken = default);
        Task DeleteAsync(PendingRegistration pending, CancellationToken cancellationToken = default);
    }
}
