namespace ImagesShop.Application.DTOs.Auth
{
    public class VerifyEmailRequestDTO
    {
        public string Email { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }
}
