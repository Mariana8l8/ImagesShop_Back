using System.ComponentModel.DataAnnotations;

namespace ImagesShop.Application.DTOs
{
    public class AddToCartRequestDTO
    {
        [Required]
        public Guid ImageId { get; set; }
    }
}
