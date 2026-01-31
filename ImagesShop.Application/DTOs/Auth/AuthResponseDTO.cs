using ImagesShop.Domain.Enums;

namespace ImagesShop.Application.DTOs.Auth
{
    public class AuthResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public Guid UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public UserRole Role { get; set; }
    }
}