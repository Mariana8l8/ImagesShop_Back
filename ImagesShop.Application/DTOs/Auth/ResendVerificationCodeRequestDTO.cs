using System.ComponentModel.DataAnnotations;

namespace ImagesShop.Application.DTOs.Auth
{
    public class ResendVerificationCodeRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
