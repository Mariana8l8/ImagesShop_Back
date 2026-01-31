using ImagesShop.Application.DTOs.Auth;

namespace ImagesShop.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken = default);
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken = default);
        Task<AuthResponseDTO> RefreshAsync(RefreshRequestDTO request, CancellationToken cancellationToken = default);
    }
}
