namespace ImagesShop.Application.DTOs
{
    public class ImageDTO
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string WatermarkedUrl { get; set; } = string.Empty;

        public string OriginalUrl { get; set; } = string.Empty;

        public Guid CategoryId { get; set; }
    }
}
