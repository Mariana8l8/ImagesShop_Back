using System;

namespace ImagesShop.Domain.Entities
{
    public class ImageTag
    {
        public Guid ImageId { get; set; }

        public Image Image { get; set; } = null!;

        public Guid TagId { get; set; }

        public Tag Tag { get; set; } = null!;
    }
}