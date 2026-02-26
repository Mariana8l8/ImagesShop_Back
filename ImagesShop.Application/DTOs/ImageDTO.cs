using System.ComponentModel.DataAnnotations;

namespace ImagesShop.Application.DTOs
{
    public class ImageDTO
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Url]
        public string WatermarkedUrl { get; set; } = string.Empty;

        [Url]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        public Guid CategoryId { get; set; }
    }
}
