using ImagesShop.API.Models;

namespace ImagesShop.API.Models
{
    public class ImageTag
    {
        public Guid ImageId { get; set; }

        public Image Image { get; set; } = null!;

        public Guid TagId { get; set; }

        public Tag Tag { get; set; } = null!;
    }
}
