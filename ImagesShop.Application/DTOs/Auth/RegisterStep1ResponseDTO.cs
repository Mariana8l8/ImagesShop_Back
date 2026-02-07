namespace ImagesShop.Application.DTOs.Auth
{
    public class RegisterStep1ResponseDTO
    {
        public string Email { get; set; } = string.Empty;

        public bool VerificationRequired { get; set; } = true;
    }
}
