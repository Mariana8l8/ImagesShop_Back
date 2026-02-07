using ImagesShop.Application.DTOs.Auth;

namespace ImagesShop.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<RegisterInitResponseDTO> RegisterAsync(RegisterStep1RequestDTO request, CancellationToken cancellationToken = default);
        Task<AuthResponseDTO> CompleteRegistrationAsync(CompleteRegistrationRequestDTO request, CancellationToken cancellationToken = default);
        Task ResendVerificationCodeAsync(ResendVerificationCodeRequestDTO request, CancellationToken cancellationToken = default);

        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken = default);
        Task<AuthResponseDTO> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDTO request, CancellationToken cancellationToken = default);
    }
}
