using System;
using System.Collections.Generic;

namespace ImagesShop.Domain.Entities
{
    public class Image
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string WatermarkedUrl { get; set; } = string.Empty;

        public string OriginalUrl { get; set; } = string.Empty;

        public Guid CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<ImageTag> Tags { get; set; } = new List<ImageTag>();

        public ICollection<Image> Users { get; set; } = new List<Image>();
    }
}