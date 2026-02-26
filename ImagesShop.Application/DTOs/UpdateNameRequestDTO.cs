using System.ComponentModel.DataAnnotations;

namespace ImagesShop.Application.DTOs
{
    public class UpdateNameRequestDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}
