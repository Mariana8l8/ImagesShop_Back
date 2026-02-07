namespace ImagesShop.Application.DTOs.Auth
{
    public class RegisterInitResponseDTO
    {
        public string Email { get; set; } = string.Empty;

        public bool VerificationRequired { get; set; }
    }
}
